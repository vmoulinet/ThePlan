/*
 * ForceOSC_TestHX711.ino
 * Sketch de test pour un seul module HX711.
 * Affiche les valeurs brutes et calibrées en Serial.
 *
 * Carte : Adafruit Feather ESP32-S3
 *
 * Connexions HX711 :
 *   DT  -> pin 12
 *   SCK -> pin 13
 *   VCC -> 3.3V (ou 5V)
 *   GND -> GND
 */

#include <HX711.h>

#define LOADCELL_DAT_PIN  12
#define LOADCELL_CLK_PIN  13

// Facteur de calibration — à ajuster selon la cellule
float CALIBRATION_FACTOR = 12205.0f;

HX711 scale;

unsigned long lastPrint = 0;
const unsigned long PRINT_INTERVAL_MS = 100;

void setup() {
  Serial.begin(115200);
  while (!Serial && millis() < 3000) {}

  Serial.println("=== Test HX711 ===");
  Serial.print("DT pin:  "); Serial.println(LOADCELL_DAT_PIN);
  Serial.print("SCK pin: "); Serial.println(LOADCELL_CLK_PIN);

  scale.begin(LOADCELL_DAT_PIN, LOADCELL_CLK_PIN);

  // Attente que le module soit prêt
  Serial.print("Attente HX711");
  unsigned long start = millis();
  while (!scale.is_ready() && millis() - start < 5000) {
    Serial.print(".");
    delay(200);
  }
  Serial.println();

  if (!scale.is_ready()) {
    Serial.println("ERREUR: HX711 non détecté !");
    Serial.println("Vérifie les connexions DT=12, SCK=13, VCC, GND.");
  } else {
    Serial.println("HX711 OK");
  }

  scale.set_scale(CALIBRATION_FACTOR);
  scale.tare();
  Serial.println("Tare effectuée. Envoi des données...");
  Serial.println("raw\tunits(kg)");
}

void loop() {
  if (!scale.is_ready()) {
    if (millis() - lastPrint > 1000) {
      lastPrint = millis();
      Serial.println("HX711 pas prêt...");
    }
    return;
  }

  if (millis() - lastPrint < PRINT_INTERVAL_MS) return;
  lastPrint = millis();

  long  raw   = scale.read();
  float units = scale.get_units(1);

  Serial.print(raw);
  Serial.print("\t");
  Serial.println(units, 3);

  // Commande "t" dans le moniteur série pour refaire une tare
  if (Serial.available()) {
    char c = Serial.read();
    if (c == 't' || c == 'T') {
      Serial.println("Tare...");
      scale.tare();
    }
  }
}
