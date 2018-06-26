/*
 Hardware:
 - Arduino UNO
 - HC-05 Bluetooth module

 Software:
 - SoftwareSerialWithHalfDuplex (Library)
   https://github.com/nickstedman/SoftwareSerialWithHalfDuplex

 Arduino Pin Mapping:
 -  2 = Bluetooth State
 -  8 = DLC/K-Line
 - 10 = Bluetooth Recieve
 - 11 = Bluetooth Transmit 
*/

#include <SoftwareSerialWithHalfDuplex.h>

SoftwareSerialWithHalfDuplex btSerial(10, 11); // Sets Recieve and Transmit pins for HC-05
SoftwareSerialWithHalfDuplex dlcSerial(8, 8, false, false); // Sets Recieve and Transmit pin for DLC/K-Line

byte obd_select = 2; // 1 = obd1, 2 = obd2

unsigned long err_timeout = 0, err_checksum = 0, ect_cnt = 0, vss_cnt = 0;

void dlcInit() {
  dlcSerial.write(0x68);
  dlcSerial.write(0x6a);
  dlcSerial.write(0xf5);
  dlcSerial.write(0xaf);
  dlcSerial.write(0xbf);
  dlcSerial.write(0xb3);
  dlcSerial.write(0xb2);
  dlcSerial.write(0xc1);
  dlcSerial.write(0xdb);
  dlcSerial.write(0xb3);
  dlcSerial.write(0xe9);
  delay(300);
}

int dlcCommand(byte cmd, byte num, byte loc, byte len, byte data[]) {
  byte crc = (0xFF - (cmd + num + loc + len - 0x01)); // checksum (FF - (cmd + num + loc + len - 0x01))

  unsigned long timeOut = millis() + 200; // timeout @ 200 ms

  dlcSerial.listen();

  dlcSerial.write(cmd);  // header/cmd read memory ??
  dlcSerial.write(num);  // num of bytes to send
  dlcSerial.write(loc);  // address
  dlcSerial.write(len);  // num of bytes to read
  dlcSerial.write(crc);  // checksum
  
  int i = 0;
  while (i < (len+3) && millis() < timeOut) {
    if (dlcSerial.available()) {
      data[i] = dlcSerial.read();
      i++;
    }
  }

  if (i < (len+3)) { // timeout
    err_timeout++;
    return 0;  // data error
  }
  // checksum
  crc = 0;
  for (i=0; i<len+2; i++) {
    crc = crc + data[i];
  }
  crc = 0xFF - (crc - 1);
  if (crc != data[len+2]) { // checksum failed
    err_checksum++;
    return 0; // data error
  }
  return 1; // success
} 

void procdlcSerial() {
  static unsigned long msTick = millis();

  if (millis() - msTick >= 250) { // run every 250 ms
    msTick = millis();

    //char h_initobd2[12] = {0x68,0x6a,0xf5,0xaf,0xbf,0xb3,0xb2,0xc1,0xdb,0xb3,0xe9}; // 200ms - 300ms delay
    //byte h_cmd1[6] = {0x20,0x05,0x00,0x10,0xcb}; // row 1
    //byte h_cmd2[6] = {0x20,0x05,0x10,0x10,0xbb}; // row 2
    //byte h_cmd3[6] = {0x20,0x05,0x20,0x10,0xab}; // row 3
    //byte h_cmd4[6] = {0x20,0x05,0x76,0x0a,0x5b}; // ecu id
    byte data[20];
    static int tps=0,rpm=0,iat=0,vss=0,volt=0,maps=0,afr=0;
    
    //static int ect=0,sft=0,lft=0,inj=0,ign=0,lmt=0,iac=0,knoc=0,baro=0,imap=0;;
    //static unsigned long vsssum=0,running_time=0,idle_time=0,distance=0;

    memset(data, 0, 20);
    if (dlcCommand(0x20,0x05,0x00,0x10,data)) { // row 1
      if (obd_select == 1) rpm = 1875000 / (data[2] * 256 + data[3] + 1); // OBD1
      if (obd_select == 2) rpm = (data[2] * 256 + data[3]) / 4; // OBD2
      // in odb1 rpm is -1
      if (rpm < 0) { rpm = 0; }

      vss = data[4];
    }

    memset(data, 0, 20);
    if (dlcCommand(0x20,0x05,0x10,0x10,data)) { // row2
      float f;
      //f = data[2];
      //f = 155.04149 - f * 3.0414878 + pow(f, 2) * 0.03952185 - pow(f, 3) * 0.00029383913 + pow(f, 4) * 0.0000010792568 - pow(f, 5) * 0.0000000015618437;
      //ect = round(f);
      f = data[3];
      f = 155.04149 - f * 3.0414878 + pow(f, 2) * 0.03952185 - pow(f, 3) * 0.00029383913 + pow(f, 4) * 0.0000010792568 - pow(f, 5) * 0.0000000015618437;
      iat = round(f);
      maps = data[4] * 0.716 - 5; // 101 kPa @ off|wot // 10kPa - 30kPa @ idle
      //baro = data[5] * 0.716 - 5;
      tps = (data[6] - 24) / 2;

      f = data[7];
      f = f / 51.3; // o2 volt in V
      
      // 0v to 1v / stock sensor
      // 0v to 5v / AEM UEGO / linear
      f = (f * 2) + 10; // afr for AEM UEGO
      afr = round(f * 10); // x10 for display w/ 1 decimal

      f = data[9];
      f = f / 10.45; // batt volt in V
      volt = round(f * 10); // x10 for display w/ 1 decimal
      //alt_fr = data[10] / 2.55
      //eld = 77.06 - data[11] / 2.5371

    }

    memset(data, 0, 20);
    if (dlcCommand(0x20,0x05,0x20,0x10,data)) { // row3
      //float f;
      //sft = (data[2] / 128 - 1) * 100; // -30 to 30
      //lft = (data[3] / 128 - 1) * 100; // -30 to 30
      
      //inj = (data[6] * 256 + data[7]) / 250; // 0 to 16
      
      //ign = (data[8] - 128) / 2;
      //f = data[8];
      //f = (f - 24) / 4;
      //ign = round(f * 10); // x10 for display w/ 1 decimal
      
      //lmt = (data[9] - 128) / 2;
      //f = data[9];
      //f = (f - 24) / 4;
      //lmt = round(f * 10); // x10 for display w/ 1 decimal
      
      //iac = data[10] / 2.55;
    }

    memset(data, 0, 20);
    if (dlcCommand(0x20,0x05,0x30,0x10,data)) { // row4
      // data[7] to data[12] unknown
      //knoc = data[14] / 51; // 0 to 5
    }

    // IMAP = RPM * MAP / IAT / 2
    // MAF = (IMAP/60)*(VE/100)*(Eng Disp)*(MMA)/(R)
    // Where: VE = 80% (Volumetric Efficiency), R = 8.314 J/°K/mole, MMA = 28.97 g/mole (Molecular mass of air)
    //float maf = 0.0;
    //imap = rpm * maps / (iat + 273) / 2;
    // ve = 75, ed = 1.595, afr = 14.7
    //maf = (imap / 60) * (80 / 100) * 1.595 * 28.9644 / 8.314472;
    // (gallons of fuel) = (grams of air) / (air/fuel ratio) / 6.17 / 454
    //gof = maf / afr / 6.17 / 454;
    //gear = vss / (rpm+1) * 150 + 0.3;

      String output = String((String)tps + ";" + (String)rpm + ";" + (String)iat + ";" + (String)vss + ";" + (String)volt + ";" + (String)maps + ";" + (String)afr);
      Serial.println(output); // debugging
      btSerial.println(output); // sends the output string towards BTclient (format: "0;0;0;0;0;0;0")
  }
}

void setup()
{
  pinMode(2,INPUT); //init Connection state from HC-05
   
  btSerial.begin(9600); // start serial connection to HC-05
  dlcSerial.begin(9600); // start serial connection to ECU/K-Line
  Serial.begin(115200); // debugging
  delay(1000); // gives serial begins time to start
  dlcInit(); // starts the initialization to the ECU
}

void loop() {
  if(digitalRead(2) == 1) // Checks if a BTclient is connected
  {
      procdlcSerial(); // Starts Requesting ECU for data
  }
  else{
    Serial.println("NoBTC"); // NO BT Client debugging
    delay(1000); // Small timeout when no Client is connected
    }
}

