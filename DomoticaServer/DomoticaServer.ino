// Arduino Domotica server with Klik-Aan-Klik-Uit-controller 
//
// By Sibbele Oosterhaven, Computer Science NHL, Leeuwarden
// V1.2, 16/12/2016, published on BB. Works with Xamarin (App: Domotica)
//
// Hardware: Arduino Uno, Ethernet shield W5100; RF transmitter on RFpin; debug LED for serverconnection on ledPin
// The Ethernet shield uses pin 10, 11, 12 and 13
// Use Ethernet2.h libary with the (new) Ethernet board, model 2
// IP address of server is based on DHCP. No fallback to static IP; use a wireless router
// Arduino server and smartphone should be in the same network segment (192.168.1.x)
// https://github.com/hjgode/homewatch/blob/master/arduino/libraries/NewRemoteSwitch/README.TXT
// kaku, Gamma, APA3, codes based on Arduino -> Voorbeelden -> NewRemoteSwitch -> ShowReceivedCode
// 1 Addr 21177114 unit 0 on/off, period: 270us   replace with your own code
// 2 Addr 21177114 unit 1 on/off, period: 270us
// 3 Addr 21177114 unit 2 on/off, period: 270us
// Supported KaKu devices -> find, download en install corresponding libraries
#define unitCodeApa3      27476178  // replace with your own code

// Include files.
#include <SPI.h>                  // Ethernet shield uses SPI-interface
#include <Ethernet.h>             // Ethernet library (use Ethernet2.h for new ethernet shield v2)
#include <NewRemoteTransmitter.h> // Remote Control, Gamma, APA3

// Set Ethernet Shield MAC address  (check yours)
byte mac[] = { 0x40, 0x6c, 0x8f, 0x36, 0x84, 0x8a }; // Ethernet adapter shield S. Oosterhaven
IPAddress ip(192, 168, 1, 3);
int ethPort = 3300;                                  // Take a free port (check your router)

#define RFPin        3  // output, pin to control the RF-sender (and Click-On Click-Off-device)
#define lowPin       5  // output, always LOW
#define highPin      6  // output, always HIGH
#define switchPin    7  // input, connected to some kind of inputswitch
#define ledPin       8  // output, led used for "connect state": blinking = searching; continuously = connected
#define infoPin      9  // output, more information
#define sensorPin0   0  // sensor0 value
#define sensorPin1   1  // sensor1 value


EthernetServer server(ethPort);              // EthernetServer instance (listening on port <ethPort>).
NewRemoteTransmitter apa3Transmitter(unitCodeApa3, RFPin, 266, 3);  // APA3 (Gamma) remote, use pin <RFPin> 

//byte actionDevice = 0;                    // Variable to store Action Device id (0, 1, 2)
bool pinState[3] = {false, false, false}; // Variable to store actual on/off state
bool pinChange = false;                   // Variable to store actual pin change
int sensorValue0 = 0;                    // Variable to store actual sensor0 value
int sensorValue1 = 0;                    // Variable to store actual sensor1 value

void setup()
{
   Serial.begin(9600);
   //while (!Serial) { ; }               // Wait for serial port to connect. Needed for Leonardo only.

   Serial.println("Domotica project, Arduino Domotica Server\n");
   
   //Init I/O-pins
   pinMode(switchPin, INPUT);            // hardware switch, for changing pin state
   pinMode(lowPin, OUTPUT);
   pinMode(highPin, OUTPUT);
   pinMode(RFPin, OUTPUT);
   pinMode(ledPin, OUTPUT);
   pinMode(infoPin, OUTPUT);
   
   //Default states
   digitalWrite(switchPin, HIGH);        // Activate pullup resistors (needed for input pin)
   digitalWrite(lowPin, LOW);
   digitalWrite(highPin, HIGH);
   digitalWrite(RFPin, LOW);
   digitalWrite(ledPin, LOW);
   digitalWrite(infoPin, LOW);

   //Try to get an IP address from the DHCP server.
   /*if (Ethernet.begin(mac) == 0)
   {
      Serial.println("Could not obtain IP-address from DHCP -> do nothing");
      while (true){     // no point in carrying on, so do nothing forevermore; check your router
      }
   }*/
   Ethernet.begin(mac, ip);
   
   //Serial.print("LED (for connect-state and pin-state) on pin "); Serial.println(ledPin);
   //Serial.print("Input switch on pin "); Serial.println(switchPin);
   Serial.println("Ethernetboard connected (pins 10, 11, 12, 13 and SPI)");
   //Serial.println("Connect to DHCP source in local network (blinking led -> waiting for connection)");
   
   //Start the ethernet server.
   server.begin();

   // Print IP-address and led indication of server state
   Serial.print("Listening address: ");
   Serial.print(Ethernet.localIP());
   
   // for hardware debug: LED indication of server state: blinking = waiting for connection
   int IPnr = getIPComputerNumber(Ethernet.localIP());   // Get computernumber in local network 192.168.1.3 -> 3)
   Serial.print(" ["); Serial.print(IPnr); Serial.print("] "); 
   Serial.print("  [Testcase: telnet "); Serial.print(Ethernet.localIP()); Serial.print(" "); Serial.print(ethPort); Serial.println("]");
   //signalNumber(ledPin, IPnr);
}

void loop()
{
   // Listen for incomming connection (app)
   EthernetClient ethernetClient = server.available();
   if (!ethernetClient) {
      //blink(ledPin);
      return; // wait for connection and blink LED
   }

   Serial.println("Application connected");
   //digitalWrite(ledPin, LOW);

   // Do what needs to be done while the socket is connected.
   while (ethernetClient.connected()) 
   {
      //checkEvent(switchPin, pinState[actionDevice]);            // update pin state
      //sensorValue0 = readSensor(0, 100);                        // update sensor0 value
      //sensorValue1 = readSensor(1, 100);                        // update sensor1 value
        
      /*if (pinChange) {
         if (pinState[actionDevice]) { digitalWrite(ledPin, HIGH); switchDefault(true); }
         else { switchDefault(false); digitalWrite(ledPin, LOW);}
         pinChange = false;
         executeCommand('s', actionDevice);
      }*/
   
      // Execute when byte is received.
      while (ethernetClient.available())
      {
         char inByte = ethernetClient.read();   // Get byte from the client.
         executeCommand(inByte);  // Wait for command to execute
         inByte = NULL;                         // Reset the read byte.
      } 
   }
   Serial.println("Application disonnected");
   
}

// Choose and switch your Kaku device, state is true/false (HIGH/LOW)
void switchDefault(byte actionDevice, bool state)
{   
   apa3Transmitter.sendUnit(actionDevice, state);          // APA3 Kaku (0/1/2, high/low)
   pinState[actionDevice] = state;    
   if(state){server.write(" ON\n");}
   else{server.write("OFF\n");}
   //Serial.println((String)"sendunitsettings: " + actionDevice + (String)" " + state);            
   //delay(100);
}

// Implementation of (simple) protocol between app and Arduino
// Request (from app) is single char ('a', 's', 't', 'i' etc.)
// Response (to app) is 4 chars  (not all commands demand a response)
void executeCommand(char cmd)
{     
         char buf[4] = {'\0', '\0', '\0', '\0'};

         // Command protocol
         Serial.print("["); Serial.print(cmd); Serial.print("] -> ");
         switch (cmd) {
         case 'a': // Report sensor value to the app  
            sensorValue0 = readSensor(0, 100);                // update sensor0 value
            intToCharBuf(sensorValue0, buf, 4);               // convert to charbuffer
            server.write(buf, 4);                             // response is always 4 chars (\n included)
            Serial.print("Sensor0: "); Serial.println(buf);
            break;
         case 'b': // Report sensor value to the app  
            sensorValue1 = readSensor(1, 100);                // update sensor1 value
            intToCharBuf(sensorValue1, buf, 4);               // convert to charbuffer
            server.write(buf, 4);                             // response is always 4 chars (\n included)
            Serial.print("Sensor1: "); Serial.println(buf);
            break;
         /*case 's': // Report switch state to the app
            if (pinState[actionDevice]) { server.write(" ON\n"); Serial.println("Pin state is ON"); }  // always send 4 chars
            else { server.write("OFF\n"); Serial.println("Pin state is OFF"); }
            break;*/
         case 'i':    
            digitalWrite(infoPin, HIGH);
            break;
            
         case 'x': //toggle device 0
            if (pinState[0]) { switchDefault(0,false); Serial.println("Set 0 state to \"OFF\""); }
            else { switchDefault(0,true); Serial.println("Set 0 state to \"ON\"");}             
            break;
            
         case 'y': //toggle device 1
            if (pinState[1]) { switchDefault(1,false); Serial.println("Set 1 state to \"OFF\"");}
            else { switchDefault(1,true); Serial.println("Set 1 state to \"ON\"");}             
            break;
            
         case 'z': //toggle device 2
            if (pinState[2]) { switchDefault(2,false); Serial.println("Set 2 state to \"OFF\"");}
            else { switchDefault(2,true); Serial.println("Set 2 state to \"ON\"");}              
            break;
            
         default:
            digitalWrite(infoPin, LOW);
         }
}

// read value from pin pn, return value is mapped between 0 and mx-1
int readSensor(byte pn, int mx)
{
  return map(analogRead(pn), 0, 1023, 0, mx);    
}

// Convert int <val> char buffer with length <len>
void intToCharBuf(int val, char buf[], byte len)
{
   String s;
   s = String(val);                        // convert tot string
   if (s.length() == 1) s = "0" + s;       // prefix redundant "0" 
   if (s.length() == 2) s = "0" + s;  
   s = s + "\n";                           // add newline
   s.toCharArray(buf, len);                // convert string to char-buffer
}

// Check switch level and determine if an event has happend
// event: low -> high or high -> low
void checkEvent(byte p, bool &state)
{
   static bool swLevel = false;       // Variable to store the switch level (Low or High)
   static bool prevswLevel = false;   // Variable to store the previous switch level

   swLevel = digitalRead(p);
   if (swLevel)
      if (prevswLevel) delay(1);
      else {               
         prevswLevel = true;   // Low -> High transition
         state = true;
         pinChange = true;
      } 
   else // swLevel == Low
      if (!prevswLevel) delay(1);
      else {
         prevswLevel = false;  // High -> Low transition
         state = false;
         pinChange = true;
      }
}

// blink led on pin <pn>
void blink(byte pn)
{
  digitalWrite(pn, HIGH); 
  delay(100); 
  digitalWrite(pn, LOW); 
  delay(100);
}

// Visual feedback on pin, based on IP number, used for debug only
// Blink ledpin for a short burst, then blink N times, where N is (related to) IP-number
/*void signalNumber(byte pin, byte n)
{
   byte i;
   for (i = 0; i < 30; i++)
       { digitalWrite(pin, HIGH); delay(20); digitalWrite(pin, LOW); delay(20); }
   delay(1000);
   for (i = 0; i < n; i++)
       { digitalWrite(pin, HIGH); delay(300); digitalWrite(pin, LOW); delay(300); }
    delay(1000);
}*/

// Convert IPAddress tot String (e.g. "192.168.1.105")
String IPAddressToString(IPAddress address)
{
    return String(address[0]) + "." + 
           String(address[1]) + "." + 
           String(address[2]) + "." + 
           String(address[3]);
}

// Returns B-class network-id: 192.168.1.3 -> 1)
/*int getIPClassB(IPAddress address)
{
    return address[2];
}*/

// Returns computernumber in local network: 192.168.1.3 -> 3)
int getIPComputerNumber(IPAddress address)
{
    return address[3];
}

// Returns computernumber in local network: 192.168.1.105 -> 5)
/*int getIPComputerNumberOffset(IPAddress address, int offset)
{
    return getIPComputerNumber(address) - offset;
}*/

