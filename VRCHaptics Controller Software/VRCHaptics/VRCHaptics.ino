#include <WiFi.h>
#include "AsyncUDP.h"
#include <EEPROM.h>
#include <esp_bt.h>
#include <esp_bt_main.h>
#include <driver/adc.h>

const int BUTTON_PIN = 36;
const int LED_PIN = 5;

const int freq = 5000;
const int resolution = 8;

const int Output01 = 13;
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

int LedFlashTimer = 0;

int Output01Time = 0;
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

int Output01Power = 0;
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

char networkName[50];
char networkPswd[50];
char deviceName[50];
int deviceNameLen = 0;
AsyncUDP udp;
const int port = 2002;
IPAddress broadcast=IPAddress(239,80,8,5);
int DeviceNameBroadcastCountup = 0;
bool thisDevice = false;
int dataStart = 0;
int dataIndex = 0;
int FactoryResetTimer = 0;
int buttonState = 0;
int ledState = 0;

void setup(){
  EEPROM.begin(512);
  // Initilize hardware:
  esp_sleep_enable_ext0_wakeup(GPIO_NUM_36,0); //Input pin 0, going low
  touch_pad_intr_disable();
  Serial.begin(115200);
  Serial.setTimeout(50);
  pinMode(BUTTON_PIN, INPUT);
  pinMode(LED_PIN, OUTPUT);
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
  
  ledcSetup(0, freq, resolution);
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

  ledcAttachPin(Output01, 0);
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
  
  detachInterrupt(0);
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

  esp_bt_controller_disable();
  esp_bluedroid_deinit();
  esp_bluedroid_disable();
  adc_power_off();
  
  WiFiReset();
  
  memset(networkName, 0, sizeof(networkName));  
  memset(networkPswd, 0, sizeof(networkPswd)); 
  memset(deviceName, 0, sizeof(deviceName));
  networkName[0] = '^';
  networkPswd[0] = '^';
  deviceNameLen = 0;

  for (int i = 0 ; i < 50 ; i++) {
    networkName[i] = EEPROM.read(i);
  }    
  for (int i = 0 ; i < 50 ; i++) {
    networkPswd[i] = EEPROM.read(i+50);
  }    
  for (int i = 0 ; i < 50 ; i++) {
    deviceName[i] = EEPROM.read(i+100);
  } 
  deviceNameLen = EEPROM.read(150);
  Serial.print("Wifi:");
  Serial.print(networkName);
  Serial.println(".");
  Serial.print("Pass:");
  Serial.print("***");
  Serial.println(".");   
  Serial.print("Device Name:");
  Serial.print(deviceName);
  Serial.println(".");

  digitalWrite(LED_PIN, HIGH);
//  digitalWrite(Output01, LOW);
//  digitalWrite(Output02, LOW);
//  digitalWrite(Output03, LOW);
//  digitalWrite(Output04, LOW);
//  digitalWrite(Output05, LOW);
//  digitalWrite(Output06, LOW);
//  digitalWrite(Output07, LOW);
//  digitalWrite(Output08, LOW);
//  digitalWrite(Output09, LOW);
//  digitalWrite(Output10, LOW);
//  digitalWrite(Output11, LOW);
//  digitalWrite(Output12, LOW);
//  digitalWrite(Output13, LOW);
//  digitalWrite(Output14, LOW);
//  digitalWrite(Output15, LOW);
//  digitalWrite(Output16, LOW);
  ledcWrite(0, 0);
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

void loop(){
  
  delay(10);
  
  LedFlashTimer = LedFlashTimer + 1;
  if (LedFlashTimer >= 400){
    LedFlashTimer = 0;
    //digitalWrite(LED_PIN, LOW);  
  }
    
  if (digitalRead(BUTTON_PIN) == LOW) {
    FactoryResetTimer = FactoryResetTimer + 1;
  }
   if (digitalRead(BUTTON_PIN) == HIGH) {      
      if (FactoryResetTimer > 500){
        digitalWrite(LED_PIN, HIGH);
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
        FactoryResetTimer = 0;
        memset(networkName, 0, sizeof(networkName));  
        memset(networkPswd, 0, sizeof(networkPswd)); 
        memset(deviceName, 0, sizeof(deviceName));
        networkName[0] = '^';
        networkPswd[0] = '^';
        deviceNameLen = 0;
        for (int i = 0 ; i < 50 ; i++) {
          EEPROM.write(i,networkName[i]);
        }    
        for (int i = 0 ; i < 50 ; i++) {
          EEPROM.write(i+50,networkPswd[i]);
        }    
        for (int i = 0 ; i < 50 ; i++) {
          EEPROM.write(i+100,deviceName[i]);
        } 
        EEPROM.write(150,deviceNameLen);
        EEPROM.commit();
        delay(500);
        Serial.println("Factory Reset...");
        WiFiReset();    
        digitalWrite(LED_PIN, HIGH); 
      }
      
      if (FactoryResetTimer > 100){
        Serial.println("Going to sleep");
        digitalWrite(LED_PIN, LOW);
        ledcWrite(0, 0);
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
        delay(5000);
        esp_deep_sleep_start();        
      }
        FactoryResetTimer = 0;
    }


  
  if (WiFi.status() != WL_CONNECTED){
     if (LedFlashTimer == 95){
      //digitalWrite(LED_PIN, HIGH);
      Serial.println("Connecting to wifi");
    }
    connectToWiFi();
  }
  if (WiFi.status() == WL_CONNECTED){
    if (LedFlashTimer == 5){
      //digitalWrite(LED_PIN, HIGH);
    }
    //Set outputs
    if (Output01Time > 0){
      ledcWrite(0, Output01Power);
      Output01Time = Output01Time - 1;
    }
    else {
      ledcWrite(0, 0);
    }
    if (Output02Time > 0){
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
     if(udp.listenMulticast(broadcast, port)) { 
        DeviceNameBroadcastCountup = DeviceNameBroadcastCountup + 1;
        if (DeviceNameBroadcastCountup > 100){ //Send Device name via multicast
          DeviceNameBroadcastCountup = 0;
          udp.print(deviceName);
        }
        
        udp.onPacket([](AsyncUDPPacket packet) {
          
              
          //Serial.print(", Data: ");
            //Serial.write(packet.data(), packet.length());
            //Serial.println();
            thisDevice = false;
            if (packet.length() == (deviceNameLen + 23)){ 
              thisDevice = true;                        
              for (int i = 0 ; i < deviceNameLen ; i++) {
                if ((char)*(packet.data()+i) != deviceName[i]){
                  thisDevice = false;
                }
              }
            }
                        
            if (thisDevice == true){

              digitalWrite(LED_PIN, ledState);
              ledState = (ledState + 1) % 2; // Flip ledState
              
              dataStart = false; 
              dataIndex = 0;             
              for (int i = 0 ; i < packet.length() ; i++){
                if ((char)*(packet.data()+i) == '&'){
                  dataStart = i + 1;
                  break;
                }
                               
              }
              //Serial.println(dataStart);
              if (packet.data()+dataStart != 0){
                Output01Time = 10;
                Output01Power = (int)*(packet.data()+dataStart);
              }  
              if (packet.data()+dataStart+1 != 0){
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
    }  
  } 
}

void connectToWiFi(){
  int countup = 0;

  if (networkName[0] == '^'){
    SerialRecieve();
    return;
  }

  printLine();
  Serial.println("Connecting to WiFi network");
  Serial.println(networkName);
  WiFi.begin(networkName, networkPswd);
  WiFi.setSleep(false);

  while (WiFi.status() != WL_CONNECTED){
    // Blink LED while we're connecting:
    digitalWrite(LED_PIN, ledState);
    ledState = (ledState + 1) % 2; // Flip ledState
    delay(500);
    countup = countup + 1;
    Serial.print(".");
    if (countup == 50){
      countup = 0;
      Serial.println("Auth Timeout...");
      memset(networkName, 0, sizeof(networkName));  
      memset(networkPswd, 0, sizeof(networkPswd)); 
      memset(deviceName, 0, sizeof(deviceName));
      networkName[0] = '^';
      networkPswd[0] = '^';
      deviceNameLen = 0;
      break;
    }
  }
  if (WiFi.status() == WL_CONNECTED){
  Serial.println();
  Serial.println("WiFi connected!");
  Serial.print("IP address: ");
  Serial.println(WiFi.localIP());
  digitalWrite(LED_PIN, HIGH); 
  delay(500);
  }
}

void SerialRecieve(){
  memset(networkName, 0, sizeof(networkName));  
  memset(networkPswd, 0, sizeof(networkPswd));
  memset(deviceName, 0, sizeof(deviceName));
  networkName[0] = '^';
  networkPswd[0] = '^';
  deviceNameLen = 0;
  char recvbytez[1000] = ""; //init serial
  memset(recvbytez, 0, sizeof(recvbytez));
  bool wifirecved = false;
  bool namedone = false;
  bool passdone = false;
  int n = 0;
  int passwordi = 0;
  int Devicei = 0;
//  Serial.println("Waiting for wifi credentials");
//  while(wifirecved == false){
//    if (digitalRead(BUTTON_PIN) == LOW) {
//      Serial.println("Backing out of Serial recieve sub");
//      break;
//    }
    if (Serial.available() > 0){ //Serial available?
      n = Serial.available(); //Size of serial available
      Serial.readBytes(recvbytez, 1000); //Read Serial
      if (recvbytez[0] == 'W' && recvbytez[1] == 'i' && recvbytez[2] == 'f'&& recvbytez[3] == 'i'){                
        wifirecved = true;
        n = n - 4;
        for (int i = 0 ; i < n ; i++) {
          if (recvbytez[i+4] == '^'){
            namedone = true;
          }
          if (recvbytez[i+4] == '&'){
            passdone = true;
          }
          if (namedone == false && recvbytez[i+4] != '^') {
            networkName[i] = recvbytez[i+4];
          }
          if (passdone == false && namedone == true && recvbytez[i+4] != '^' && recvbytez[i+4] != '&') {
            networkPswd[passwordi] = recvbytez[i+4];
            passwordi = passwordi + 1;
          } 
          if (passdone == true && namedone == true && recvbytez[i+4] != '^' && recvbytez[i+4] != '&') {
            deviceName[Devicei] = recvbytez[i+4];
            Devicei = Devicei + 1;
            deviceNameLen = Devicei;
          } 
        }
        Serial.print("Serial Recieved Wifi:");
        Serial.print(networkName);
        Serial.println(".");
        Serial.print("Pass:");
        Serial.print(networkPswd);
        Serial.println(".");   
        Serial.print("Device Name:");
        Serial.print(deviceName);
        Serial.println(".");
        for (int i = 0 ; i < 50 ; i++) {
          EEPROM.write(i,networkName[i]);
        }    
        for (int i = 0 ; i < 50 ; i++) {
          EEPROM.write(i+50,networkPswd[i]);
        }    
        for (int i = 0 ; i < 50 ; i++) {
          EEPROM.write(i+100,deviceName[i]);
        } 
        EEPROM.write(150,deviceNameLen);
        EEPROM.commit();     
      }
    }
//  }
}

void printLine(){
  Serial.println();
  for (int i=0; i<30; i++)
    Serial.print("-");
  Serial.println();
}

void WiFiReset(){
    WiFi.persistent(false);
    WiFi.disconnect(true);
    delay(1000);
    WiFi.softAPdisconnect(true);
    delay(1000);
    Serial.print("WIFI status = ");
    Serial.println(WiFi.getMode());
    WiFi.mode(WIFI_STA);
    delay(1000);
    WiFi.setTxPower(WIFI_POWER_MINUS_1dBm);
    Serial.print("WIFI status = ");
    Serial.println(WiFi.getMode());
}
