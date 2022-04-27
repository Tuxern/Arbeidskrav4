// SSC_Test modifisert utgave for arbeidskrav #1 i OOP og DB.
// Denne versjonen returnerer kommandoen som ble benyttet som 
// første del av svaret. Denne kan benyttes istedet for det
// opprinnelige arduinoprogrammet. Denne kan forenkle C# koden,
// Ved at man kan teste på hvilken kode som har blitt benyttet.
// Eksempel på bruk.
// Kommando: readconf   
// Svar :    readconf;C385IT001;0.0;500.0;40;440


//Endret for Arbeidskrav 4, lagd til read kommando for å kunne lese av all data med 1 kommando
//#include "Ticker.h"
//void connection()
//{
//  Serial.println("Connected");
//}

int analogPin_1 = A1;
int analogPin_0 = A0;
int analogPin_5 = A5; //Jobs testing
int analogPin_3 = A3; //Jobs testing
int vAB = 0;
int vA = 0;
int vB = 0;
String inputString = "";
bool stringComplete = false;
String password = "password";                             //Device Password
float scaled = 0;
float lrv = 0.0;                                          //Low Range Value 
float urv = 500.0;                                        //Upper Range Value
int alarmL = 40;                                          //Alarm Low
int alarmH = 440;                                         //Alarm High
String deviceName = "AXP-3000";                          //Device Name
String configValues = "AXP-3000;0.0;500.0;40;440";       //Saved Configuration Name and Values as String
//Ticker tick(connection, 3000);
void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);           //  setup serial
//  tick.start();
  pinMode(10, OUTPUT);
}

void loop() {
//tick.update();
  if (stringComplete)
  {
      vA = analogRead(analogPin_1);     //Reads analog data from Pin 1
          vB = analogRead (analogPin_0);    //Reads analog data from pin 0
          vAB = vA  - vB;
          scaled = urv/1023.0*vAB;
  

      if (inputString.substring(0,7) == "readraw" || inputString.substring(0,10) == "readscaled"||inputString.substring(0,10) == "readstatus" ||inputString.substring(0,4) == "readalldata")
      {
        vA = analogRead(analogPin_1);     //Reads analog data from Pin 1
        vB = analogRead (analogPin_0);    //Reads analog data from pin 0
        vAB = vA  - vB;
        scaled = urv/1023.0*vAB;
      }
      
      if (inputString.substring(0,8) == "readconf")
      {
        Serial.println("readconf;" + configValues);   //Print Configuration string 
      }
      
      else if (inputString.substring(0,11) == "readalldata")
      {
        if (scaled > alarmL && scaled < alarmH)
        {
          Serial.println("read;" + String("0;") + String(vAB) +";" + String(scaled));    //print Ok message
        }
        else if(scaled > 1000 || scaled == 0)
        {
          Serial.println("read;" + String("1;") + String(vAB) +";" + String(scaled));    //Print Fail messege
        }
        else if (scaled < alarmL)
        {
          Serial.println("read;" + String("2;") + String(vAB) +";" + String(scaled));    //Print Alarm Low message
        }
        else if (scaled > alarmH)
        {
          Serial.println("read;" + String("3;") + String(vAB) +";" + String(scaled));    //Print Alarm Low message
        }
      }
       
      else if (inputString.substring(0,10) == "readscaled")
      {
        Serial.println("readscaled;" + String(scaled));   //Print Scaled data
      }
      else if (inputString.substring(0,7) == "readraw")
      {
        
        Serial.println("readraw;" + String(vAB));    //Print Raw data
      }
      else if (inputString.substring(0,10) == "readstatus")
      {
        if (scaled > alarmL && scaled < alarmH)
        {
          Serial.println("readstatus;" + String("0"));    //print Ok message
        }
        else if(scaled > 1000 || scaled == 0)
        {
          Serial.println("readstatus;" + String("1"));    //Print Fail messege
        }
        else if (scaled < alarmL)
        {
          Serial.println("readstatus;" + String("2"));    //Print Alarm Low message
        }
        else if (scaled > alarmH)
        {
          Serial.println("readstatus;" + String("3"));    //Print Alarm High message
        }
      }
      else if (inputString.substring(0,9) == "writeconf")
      {
        if (inputString.substring(10,10 + password.length()) == password)   //Checks if incoming password is correct
        {
          configValues = getValue(inputString, '>', 2);                     //Separets "writeconf" and ">password<" from the inputString String, and saves the String as configValues
          deviceName = getValue(configValues, ';', 0);                      //Gets the Device Name from the String
          String slrv = getValue(configValues, ';', 1);                     //Gets the Low Range Value from the String 
          String surv = getValue(configValues, ';', 2);                     //Gets the Upper Range Value from the String 
          String salarmL = getValue(configValues, ';', 3);                  //Gets Alarm Low from the String 
          String salarmH = getValue(configValues, ';', 4);                  //Gets Alarm Low from the String 
          
          lrv = slrv.toFloat();                                             //Sets Low Range Value
          urv = surv.toFloat();                                             //Sets Upper Range Value
          alarmL = salarmL.toInt();                                         //Sets Alarm Low Value
          alarmH = salarmH.toInt();                                         //Sets Alarm High Value
     
          Serial.println("writeconf;" + String("1"));                                                
          
        }
        else
        {
          Serial.println("writeconf;" + String("0"));
        }
      }
      else if (inputString.substring(0, 6) == "output")
      {
        if (inputString.substring(7, 8) == "e")
        {
          digitalWrite(10, HIGH);  
        }
        else if (inputString.substring(7, 8) == "f")
        {
          digitalWrite(10, LOW);
        }
      }
      else // Hvis ikke noe annet slår til - for test
        {
          Serial.println("You failed! Try again...?");
        }
      inputString = "";
      stringComplete = false;
  }
}

void serialEvent()
{
  while (Serial.available())
  {
    inputString = Serial.readString();
    stringComplete = true;
  }
}



//This Chunk of code takes a string and separates it based on a given character and returns The item between the separating character

String getValue(String data, char separator, int index)
{
    int found = 0;
    int strIndex[] = { 0, -1 };
    int maxIndex = data.length() - 1;

    for (int i = 0; i <= maxIndex && found <= index; i++) {
        if (data.charAt(i) == separator || i == maxIndex) {
            found++;
            strIndex[0] = strIndex[1] + 1;
            strIndex[1] = (i == maxIndex) ? i+1 : i;
        }
    }
    return found > index ? data.substring(strIndex[0], strIndex[1]) : "";
}
