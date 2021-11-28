//VRCHaptics controller code, started by Pillazo

#include <WiFi.h> //This area calls out subprograms for reference
#include "AsyncUDP.h"
#include <EEPROM.h>
#include <esp_bt.h>
#include <esp_bt_main.h>
#include <driver/adc.h>

const int BUTTON_PIN = 36; //Pin 36 of the ESP32 is where the pushbutton is assigned
const int LED_PIN = 5; //The LED pin of the ESP32 is already wired to the board

const int freq = 5000;  //truly a random number, sounded good
const int resolution = 8; //eight bits, easy peasy

const int Output01 = 13;  //Assigning the ESP32 pins to my order of outputs
const int Output02 = 12;
const int Output03 = 14;
const int Output04 = 27;
const int Output05 = 26;
const int Output06 = 25;
const int Output07 = 33;
const int Output08 = 32;
const int Output09 = 21;
const int Output10 = 23;
const int Output11 = 18;
const int Output12 = 15;
const int Output13 = 2;
const int Output14 = 4;
const int Output15 = 17;
const int Output16 = 16;

int LedFlashTimer = 0;  //This was kind of a debug thing

int Output01Time = 0; //Each pin is given a signal to start, for a certain length of time, once its reached zero, it turns off
int Output02Time = 0;
int Output03Time = 0;
int Output04Time = 0;
int Output05Time = 0;
int Output06Time = 0;
int Output07Time = 0;
int Output08Time = 0;
int Output09Time = 0;
int Output10Time = 0;
int Output11Time = 0;
int Output12Time = 0;
int Output13Time = 0;
int Output14Time = 0;
int Output15Time = 0;
int Output16Time = 0;

int Output01Power = 0; //Intensity of each output, as given by the VB.net program
int Output02Power = 0;
int Output03Power = 0;
int Output04Power = 0;
int Output05Power = 0;
int Output06Power = 0;
int Output07Power = 0;
int Output08Power = 0;
int Output09Power = 0;
int Output10Power = 0;
int Output11Power = 0;
int Output12Power = 0;
int Output13Power = 0;
int Output14Power = 0;
int Output15Power = 0;
int Output16Power = 0;

char networkName[50]; //The wifi SSID (wifi name)
char networkPswd[50]; //Wifi password
char deviceName[50];  //The name given to the device so it can recognize its packets
int deviceNameLen = 0; //Another quick tool to help it recognize its own packets
AsyncUDP udp; //Call out the UDP for use
const int port = 2002; // Random port I pulled out of the air
IPAddress broadcast=IPAddress(239,80,8,5); //Randon IP in the multicast range I pulled out the air too
int DeviceNameBroadcastCountup = 0; //This is a counter/timer for every now and then spitting the device name out to the UDP, so the VB.net program can see the controller and mark it green
bool thisDevice = false; //The 'is this packet for this device?' bit
int dataStart = 0;  //Start point in the packet for the actual outputs after the name
int dataIndex = 0;  //Indexing through the data
int FactoryResetTimer = 0; //Used for the button press, if pressed, add time to this timer
int ledState = 0; //Used for flipping the LED during connection

void setup(){   //Actual start of code, this is the setup area
  EEPROM.begin(512);  //Needed to store data on the ESP32, defines 512 bytes
  // Initilize hardware:
  esp_sleep_enable_ext0_wakeup(GPIO_NUM_36,0); //Input pin 36, going low, wakes up the device from sleep
  touch_pad_intr_disable(); //Disable all the touch capability of the ESP32
  Serial.begin(115200); //Setup communication to the computer via USB
  Serial.setTimeout(50);  //Timeout cause timeout, ya know?
  pinMode(BUTTON_PIN, INPUT); //The push button is an input to the ESP32 lol
  pinMode(LED_PIN, OUTPUT); //The LED and all the outputs are outs of the ESP32 lol
  pinMode(Output01, OUTPUT);
  pinMode(Output02, OUTPUT);
  pinMode(Output03, OUTPUT);
  pinMode(Output04, OUTPUT);
  pinMode(Output05, OUTPUT);
  pinMode(Output06, OUTPUT);
  pinMode(Output07, OUTPUT);
  pinMode(Output08, OUTPUT);
  pinMode(Output09, OUTPUT);
  pinMode(Output10, OUTPUT);
  pinMode(Output11, OUTPUT);
  pinMode(Output12, OUTPUT);
  pinMode(Output13, OUTPUT);
  pinMode(Output14, OUTPUT);
  pinMode(Output15, OUTPUT);
  pinMode(Output16, OUTPUT);
  
  ledcSetup(0, freq, resolution); //This area sets up the PWM of the outputs, allowing them to have intensity
  ledcSetup(1, freq, resolution);
  ledcSetup(2, freq, resolution);
  ledcSetup(3, freq, resolution);
  ledcSetup(4, freq, resolution);
  ledcSetup(5, freq, resolution);
  ledcSetup(6, freq, resolution);
  ledcSetup(7, freq, resolution);
  ledcSetup(8, freq, resolution);
  ledcSetup(9, freq, resolution);
  ledcSetup(10, freq, resolution);
  ledcSetup(11, freq, resolution);
  ledcSetup(12, freq, resolution);
  ledcSetup(13, freq, resolution);
  ledcSetup(14, freq, resolution);
  ledcSetup(15, freq, resolution);

  ledcAttachPin(Output01, 0); //Previous section just parameterized the intensity stuff, this actually assigns them to the outputs
  ledcAttachPin(Output02, 1);
  ledcAttachPin(Output03, 2);
  ledcAttachPin(Output04, 3);
  ledcAttachPin(Output05, 4);
  ledcAttachPin(Output06, 5);
  ledcAttachPin(Output07, 6);
  ledcAttachPin(Output08, 7);
  ledcAttachPin(Output09, 8);
  ledcAttachPin(Output10, 9);
  ledcAttachPin(Output11, 10);
  ledcAttachPin(Output12, 11);
  ledcAttachPin(Output13, 12);
  ledcAttachPin(Output14, 13);
  ledcAttachPin(Output15, 14);
  ledcAttachPin(Output16, 15);
  
  detachInterrupt(0); //Any interrupt can go away
  detachInterrupt(1);
  detachInterrupt(2);
  detachInterrupt(3);
  detachInterrupt(4);
  detachInterrupt(5);
  detachInterrupt(12);
  detachInterrupt(13);
  detachInterrupt(14);
  detachInterrupt(15);
  detachInterrupt(16);
  detachInterrupt(17);
  detachInterrupt(18);
  detachInterrupt(19);
  detachInterrupt(21);
  detachInterrupt(22);
  detachInterrupt(23);
  detachInterrupt(25);
  detachInterrupt(26);
  detachInterrupt(27);
  detachInterrupt(32);
  detachInterrupt(33);
  detachInterrupt(34);
  detachInterrupt(35);
  detachInterrupt(36);
  detachInterrupt(37);
  detachInterrupt(38);
  detachInterrupt(39);

  esp_bt_controller_disable();  //Disable all bluetooth
  esp_bluedroid_deinit(); 
  esp_bluedroid_disable();
  adc_power_off();  //Disable the analog to digital conversion stuff too
  
  WiFiReset();  //This is a subroutine at the end of the program, resets the wifi
  
  memset(networkName, 0, sizeof(networkName));  //Clear out the network SSID, password and device name to defaults
  memset(networkPswd, 0, sizeof(networkPswd)); 
  memset(deviceName, 0, sizeof(deviceName));
  networkName[0] = '^';
  networkPswd[0] = '^';
  deviceNameLen = 0;

  for (int i = 0 ; i < 50 ; i++) {  //Now that its cleared, read in the stuff from memory
    networkName[i] = EEPROM.read(i);
  }    
  for (int i = 0 ; i < 50 ; i++) {
    networkPswd[i] = EEPROM.read(i+50);
  }    
  for (int i = 0 ; i < 50 ; i++) {
    deviceName[i] = EEPROM.read(i+100);
  } 
  deviceNameLen = EEPROM.read(150);
  Serial.print("Wifi:");  //Spit it out of the USB communication
  Serial.print(networkName);
  Serial.println(".");
  Serial.print("Pass:");
  Serial.print("***");
  Serial.println(".");   
  Serial.print("Device Name:");
  Serial.print(deviceName);
  Serial.println(".");

  digitalWrite(LED_PIN, HIGH);  //Turn on the LED
  ledcWrite(0, 0);  //Tell all the intensity stuff to be off
  ledcWrite(1, 0);
  ledcWrite(2, 0);
  ledcWrite(3, 0);
  ledcWrite(4, 0);
  ledcWrite(5, 0);
  ledcWrite(6, 0);
  ledcWrite(7, 0);
  ledcWrite(8, 0);
  ledcWrite(9, 0);
  ledcWrite(10, 0);
  ledcWrite(11, 0);
  ledcWrite(12, 0);
  ledcWrite(13, 0);
  ledcWrite(14, 0);
  ledcWrite(15, 0);
  
}

void loop(){  //Main loop of the program! Starts after the setup code above
  
  delay(10);  //Default delay, no reason to run like crazy
  
  LedFlashTimer = LedFlashTimer + 1;  //Debug stuff, can probably ignore it, may remove later
  if (LedFlashTimer >= 400){
    LedFlashTimer = 0;
    //digitalWrite(LED_PIN, LOW);  
  }
    
  if (digitalRead(BUTTON_PIN) == LOW) { //Default the button is held high! so this means its being pressed, add to the timer!
    FactoryResetTimer = FactoryResetTimer + 1;
  }
   if (digitalRead(BUTTON_PIN) == HIGH) { //If the button is let go, see where timer ended up     
      if (FactoryResetTimer > 500){ //If its more than 5 seconds (500count x 10ms delay = 5s), then run factory reset
        digitalWrite(LED_PIN, HIGH);  //Blink the LED twice just cause
        delay(100);
        digitalWrite(LED_PIN, LOW);
        delay(500);
        digitalWrite(LED_PIN, HIGH);
        delay(100);
        digitalWrite(LED_PIN, LOW);
        delay(500);
        digitalWrite(LED_PIN, HIGH);
        delay(100);
        digitalWrite(LED_PIN, LOW);
        delay(500);
        FactoryResetTimer = 0;  //Clear the timer
        memset(networkName, 0, sizeof(networkName)); //Reset the network name, password and device name  
        memset(networkPswd, 0, sizeof(networkPswd)); 
        memset(deviceName, 0, sizeof(deviceName));
        networkName[0] = '^'; //Give them default values
        networkPswd[0] = '^';
        deviceNameLen = 0;
        for (int i = 0 ; i < 50 ; i++) {  //Also clear out the device memory too so it doesn't start up with old values
          EEPROM.write(i,networkName[i]);
        }    
        for (int i = 0 ; i < 50 ; i++) {
          EEPROM.write(i+50,networkPswd[i]);
        }    
        for (int i = 0 ; i < 50 ; i++) {
          EEPROM.write(i+100,deviceName[i]);
        } 
        EEPROM.write(150,deviceNameLen);
        EEPROM.commit();  //Apply that stuff to memory
        delay(500); //Wait half second
        Serial.println("Factory Reset..."); //Spit data to USB
        WiFiReset();    //Reset Wifi so its no longer connected to old stuff if it was
        digitalWrite(LED_PIN, HIGH); //Set LED high
      }
      
      if (FactoryResetTimer > 10){ //If it was simply just longer than 0.1 second, then put the device to sleep
        Serial.println("Going to sleep"); //Send sleep message to USB
        digitalWrite(LED_PIN, LOW); //Turn off LED
        ledcWrite(0, 0);  //Turn off all outputs
        ledcWrite(1, 0);
        ledcWrite(2, 0);
        ledcWrite(3, 0);
        ledcWrite(4, 0);
        ledcWrite(5, 0);
        ledcWrite(6, 0);
        ledcWrite(7, 0);
        ledcWrite(8, 0);
        ledcWrite(9, 0);
        ledcWrite(10, 0);
        ledcWrite(11, 0);
        ledcWrite(12, 0);
        ledcWrite(13, 0);
        ledcWrite(14, 0);
        ledcWrite(15, 0); 
        udp.close();       
        delay(5000); //Wait 5 whole seconds
        esp_deep_sleep_start(); //Put device to sleep, when it wakes up by the push button it starts at the top of setup
      }
        FactoryResetTimer = 0; //If it wasn't pressed long enough for any of these, just clear it out to start it over for next time
    }


  
  if (WiFi.status() != WL_CONNECTED){ //If device is not yet connected to wifi...
     if (LedFlashTimer == 95){ //Debug stuff, probably remove later
      //digitalWrite(LED_PIN, HIGH);
      Serial.println("Connecting to wifi"); //Give info out USB
    }
    connectToWiFi();  //Call wifi connection sub routine (see below)
  }
  if (WiFi.status() == WL_CONNECTED){ //If it is connected though!
    if (LedFlashTimer == 5){  //Debug stuff, probably remove later
      //digitalWrite(LED_PIN, HIGH);
    }
    //Set outputs
    if (Output01Time > 0){  //If the packet saw that this output was on, it gives it a timer value to run with
      ledcWrite(0, Output01Power);  //Place the intensity into the output and it'll start buzzing
      Output01Time = Output01Time - 1;  //Start decreasing the timer for this output
    }
    else {  //If there's no more time left...
      ledcWrite(0, 0);  //Turn off the output by setting intensity to zero
    }
    if (Output02Time > 0){  //repeat for all other 16 outputs!
      ledcWrite(1, Output02Power);
      Output02Time = Output02Time - 1;
    }
    else {
      ledcWrite(1, 0);
    }
    if (Output03Time > 0){
      ledcWrite(2, Output03Power);
      Output03Time = Output03Time - 1;
    }
    else {
      ledcWrite(2, 0);
    }
    if (Output04Time > 0){
      ledcWrite(3, Output04Power);
      Output04Time = Output04Time - 1;
    }
    else {
      ledcWrite(3, 0);
    }
    if (Output05Time > 0){
      ledcWrite(4, Output05Power);
      Output05Time = Output05Time - 1;
    }
    else {
      ledcWrite(4, 0);
    }
    if (Output06Time > 0){
      ledcWrite(5, Output06Power);
      Output06Time = Output06Time - 1;
    }
    else {
      ledcWrite(5, 0);
    }
    if (Output07Time > 0){
      ledcWrite(6, Output07Power);
      Output07Time = Output07Time - 1;
    }
    else {
      ledcWrite(6, 0);
    }
    if (Output08Time > 0){
      ledcWrite(7, Output08Power);
      Output08Time = Output08Time - 1;
    }
    else {
      ledcWrite(7, 0);
    }
    if (Output09Time > 0){
      ledcWrite(8, Output09Power);
      Output09Time = Output09Time - 1;
    }
    else {
      ledcWrite(8, 0);
    }
    if (Output10Time > 0){
      ledcWrite(9, Output10Power);
      Output10Time = Output10Time - 1;
    }
    else {
      ledcWrite(9, 0);
    }
    if (Output11Time > 0){
      ledcWrite(10, Output11Power);
      Output11Time = Output11Time - 1;
    }
    else {
      ledcWrite(10, 0);
    }
    if (Output12Time > 0){
      ledcWrite(11, Output12Power);
      Output12Time = Output12Time - 1;
    }
    else {
      ledcWrite(11, 0);
    }
    if (Output13Time > 0){
      ledcWrite(12, Output13Power);
      Output13Time = Output13Time - 1;
    }
    else {
      ledcWrite(12, 0);
    }
    if (Output14Time > 0){
      ledcWrite(13, Output14Power);
      Output14Time = Output14Time - 1;
    }
    else {
      ledcWrite(13, 0);
    }
    if (Output15Time > 0){
      ledcWrite(14, Output15Power);
      Output15Time = Output15Time - 1;
    }
    else {
      ledcWrite(14, 0);
    }
    if (Output16Time > 0){
      ledcWrite(15, Output16Power);
      Output16Time = Output16Time - 1;
    }
    else {
      ledcWrite(15, 0);
    }
     
// Recieve Packet
     if(udp.listenMulticast(broadcast, port)) { //If the network is setup (UDP connected to multicast)

        udp.onPacket([](AsyncUDPPacket packet) {  //Get packet from network!
          
            thisDevice = false; //Start with saying this packet isn't for this device
            if (packet.length() >= (deviceNameLen + 23)){  //However if the device's name length in characters and the rest of the standard packet match...
              thisDevice = true; //Set it to a tentative maybe
              for (int i = 0 ; i < deviceNameLen ; i++) { //Go through each character of the name
                if ((char)*(packet.data()+i) != deviceName[i]){ //If it doesn't match
                  thisDevice = false; //Then set that this packet isn't for this device
                }
              }
            }
                        
            if (thisDevice == true){  //If we've determined that this packet is for this device then...

              digitalWrite(LED_PIN, ledState);  //Flip the LED output
              ledState = (ledState + 1) % 2; // Flip the LED state
              
              dataStart = 0; //Clear out data start position
              dataIndex = 0; //Clear out data index position
              for (int i = 0 ; i < packet.length() ; i++){  //For every byte of the packet
                if ((char)*(packet.data()+i) == '&'){ //If we find the character '&' then
                  dataStart = i + 1;  //Set this as the data starting position
                  break;  //Exit this loop that looks for the '&'
                }
                               
              }
              //Serial.println(dataStart);  //Debug
              if (packet.data()+dataStart != 0){ //So if the first byte doesn't equal a hard zero, add a time value to the timer so this output will begin buzzing!
                Output01Time = 10;  //Adding essentially 100ms (0.1 seconds)
                Output01Power = (int)*(packet.data()+dataStart);  //This converts the byte to an intensity (0-255 where 0 is off, 255 is full power)
              }  
              if (packet.data()+dataStart+1 != 0){  //Repeat for the rest of the bytes of each output
                Output02Time = 10;
                Output02Power = (int)*(packet.data()+dataStart+1);
              }
              if (packet.data()+dataStart+2 != 0){
                Output03Time = 10;
                Output03Power = (int)*(packet.data()+dataStart+2);
              }
              if (packet.data()+dataStart+3 != 0){
                Output04Time = 10;
                Output04Power = (int)*(packet.data()+dataStart+3);
              }
              if (packet.data()+dataStart+4 != 0){
                Output05Time = 10;
                Output05Power = (int)*(packet.data()+dataStart+4);
              }
              if (packet.data()+dataStart+5 != 0){
                Output06Time = 10;
                Output06Power = (int)*(packet.data()+dataStart+5);
              }
              if (packet.data()+dataStart+6 != 0){
                Output07Time = 10;
                Output07Power = (int)*(packet.data()+dataStart+6);
              }
              if (packet.data()+dataStart+7 != 0){
                Output08Time = 10;
                Output08Power = (int)*(packet.data()+dataStart+7);
              }
              if (packet.data()+dataStart+8 != 0){
                Output09Time = 10;
                Output09Power = (int)*(packet.data()+dataStart+8);
              }
              if (packet.data()+dataStart+9 != 0){
                Output10Time = 10;
                Output10Power = (int)*(packet.data()+dataStart+9);
              }
              if (packet.data()+dataStart+10 != 0){
                Output11Time = 10;
                Output11Power = (int)*(packet.data()+dataStart+10);
              }
              if (packet.data()+dataStart+11 != 0){
                Output12Time = 10;
                Output12Power = (int)*(packet.data()+dataStart+11);
              }
              if (packet.data()+dataStart+12 != 0){
                Output13Time = 10;
                Output13Power = (int)*(packet.data()+dataStart+12);
              }
              if (packet.data()+dataStart+13 != 0){
                Output14Time = 10;
                Output14Power = (int)*(packet.data()+dataStart+13);
              }
              if (packet.data()+dataStart+14 != 0){
                Output15Time = 10;
                Output15Power = (int)*(packet.data()+dataStart+14);
              }
              if (packet.data()+dataStart+15 != 0){
                Output16Time = 10;
                Output16Power = (int)*(packet.data()+dataStart+15);
              }       
            }         
        });
        DeviceNameBroadcastCountup = DeviceNameBroadcastCountup + 1;  //Add time to timer for spitting name out for VB.net program to find
        if (DeviceNameBroadcastCountup > 100){ //Send Device name via multicast
          DeviceNameBroadcastCountup = 0; //Reset timer first
          udp.print(deviceName);  //Then spit device name out to network
          udp.broadcast(deviceName);
          digitalWrite(LED_PIN, ledState);  //Flip the LED output
          ledState = (ledState + 1) % 2; // Flip the LED state
        }
    }  
  } 
}

void connectToWiFi(){ //This sets up the wifi connection
  int countup = 0;  //A timer to before it gives up, with the code below its like 25 seconds-ish

  if (networkName[0] == '^'){ //First check we have a real name and not the default name for a network SSID
    SerialRecieve();  //If we do, go to USB for a legit one
    return; // once you've left that, gotten one or not, return back to the top, if it is legit, it will skip to the next part, if not, this is repeated
  }

  printLine();  //Make blank line
  Serial.println("Connecting to WiFi network"); //Spit to USB 'connecting'
  Serial.println(networkName);  //Attach the wifi name we're trying to connect to
  WiFi.begin(networkName, networkPswd); //Actual command to the wifi stuff to try and connect
  WiFi.setSleep(false); //Don't let the wifi go to sleep mode

  while (WiFi.status() != WL_CONNECTED){  //If the wifi has not yet connected, remain in this loop
    digitalWrite(LED_PIN, ledState); // Blink LED while we're connecting:
    ledState = (ledState + 1) % 2; // Flip the LED state
    delay(500); //Wait half second
    countup = countup + 1;  //Add time to our timeout
    Serial.print(".");  //Dots dots dots, more dots... ok stops dots
    if (countup == 50){ //If timeout happens
      countup = 0;  //Reset timeout
      Serial.println("Auth Timeout..."); //Tell user the timeout has happened
      memset(networkName, 0, sizeof(networkName));  //Wipe out the wifi credentials to try again
      memset(networkPswd, 0, sizeof(networkPswd)); 
      memset(deviceName, 0, sizeof(deviceName));
      networkName[0] = '^';
      networkPswd[0] = '^';
      deviceNameLen = 0;
      break;  //Exit this 'while' loop cause of timeout
    }
  }
  if (WiFi.status() == WL_CONNECTED){ //However if the wifi was successful
  Serial.println(); //Blank line to USB
  Serial.println("WiFi connected!"); //Tell user it was connected
  Serial.print("IP address: "); //IP is..
  Serial.println(WiFi.localIP()); // Give device's IP
  digitalWrite(LED_PIN, HIGH); //Set LED on
  delay(500); //wait half second
  }
}

void SerialRecieve(){ //This is where the device listens to the USB for wifi name, password and device name
  memset(networkName, 0, sizeof(networkName));  //First clear out any old name
  memset(networkPswd, 0, sizeof(networkPswd));
  memset(deviceName, 0, sizeof(deviceName));
  networkName[0] = '^';
  networkPswd[0] = '^';
  deviceNameLen = 0;
  char recvbytez[1000] = ""; //init serial
  memset(recvbytez, 0, sizeof(recvbytez));  //Clear out this section of data
  bool wifirecved = false;  //Clear out some other variables used
  bool namedone = false;
  bool passdone = false;
  int n = 0;
  int passwordi = 0;
  int Devicei = 0;
  
    if (Serial.available() > 0){ //Serial available?
      n = Serial.available(); //Size of serial available
      Serial.readBytes(recvbytez, 1000); //Read Serial
      if (recvbytez[0] == 'W' && recvbytez[1] == 'i' && recvbytez[2] == 'f'&& recvbytez[3] == 'i'){    //Serial contains wifi info            
        wifirecved = true;  //Got wifi info!
        n = n - 4;  //Starting data off minus the 'wifi'
        for (int i = 0 ; i < n ; i++) { //For all the bytes in the serial packet
          if (recvbytez[i+4] == '^'){ //If its '^'...
            namedone = true; //Then the name has finished
          }
          if (recvbytez[i+4] == '&'){ //If its '&'...
            passdone = true; //Then the password is done
          }
          if (namedone == false && recvbytez[i+4] != '^') { //going through the loop, if the name isn't done...
            networkName[i] = recvbytez[i+4];  //Then create the network name, character by character
          }
          if (passdone == false && namedone == true && recvbytez[i+4] != '^' && recvbytez[i+4] != '&') {  //If we've got the name and we havne't met the delimiters yet
            networkPswd[passwordi] = recvbytez[i+4];  //Create the password, characater by character
            passwordi = passwordi + 1; //Increase password length
          } 
          if (passdone == true && namedone == true && recvbytez[i+4] != '^' && recvbytez[i+4] != '&') { //If we've got the SSID and the password and haven't met delimiters then
            deviceName[Devicei] = recvbytez[i+4]; //Create device name, character by character
            Devicei = Devicei + 1;  //Increase name length
            deviceNameLen = Devicei;
          } 
        }
        Serial.print("Serial Recieved Wifi:");  //Tell user we got the info
        Serial.print(networkName);
        Serial.println(".");
        Serial.print("Pass:");
        Serial.print(networkPswd);
        Serial.println(".");   
        Serial.print("Device Name:");
        Serial.print(deviceName);
        Serial.println(".");
        for (int i = 0 ; i < 50 ; i++) {  //Set the SSID, password and device name to memory
          EEPROM.write(i,networkName[i]);
        }    
        for (int i = 0 ; i < 50 ; i++) {
          EEPROM.write(i+50,networkPswd[i]);
        }    
        for (int i = 0 ; i < 50 ; i++) {
          EEPROM.write(i+100,deviceName[i]);
        } 
        EEPROM.write(150,deviceNameLen);
        EEPROM.commit();     //Set memory
      }
    }
//  }
}

void printLine(){ //Make a line of dashes like ----------
  Serial.println(); //Blank line
  for (int i=0; i<30; i++) //Do this 30 times
    Serial.print("-"); //Print 1 dash
  Serial.println(); //Blank line
}

void WiFiReset(){ //Wifi reset
    WiFi.persistent(false); //Disable the persistance
    WiFi.disconnect(true);  //Disconnect
    delay(1000);  //Wait 1 second
    WiFi.softAPdisconnect(true);  //Disconnect some more!
    delay(1000);  //Wait 1 second
    Serial.print("WIFI status = "); //Tell user wifi is off
    Serial.println(WiFi.getMode());
    WiFi.mode(WIFI_STA);  //Setup as a wifi device
    delay(1000);  //Wait 1 second
    WiFi.setTxPower(WIFI_POWER_MINUS_1dBm); //Set that wifi antenna to FULL POWER!
    Serial.print("WIFI status = ");   //Tell user wifi is on
    Serial.println(WiFi.getMode());
}
