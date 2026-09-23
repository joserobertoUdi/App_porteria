import 'package:flutter/material.dart';

class AppTheme {
  //colores de la udi
  static const Color udiRed = Color(0xFFD50000);
  static const Color udiDarkRed = Color(0xFFA30000);
  static const Color white = Colors.white;

  //degradado del fondo (sera usado en el login)
  static const LinearGradient udiGradient = LinearGradient(
    begin: Alignment.topCenter,
    end: Alignment.bottomCenter,
    colors: [
      udiDarkRed,
      udiRed,
      ],
  );

  //configuracion plural del tema
  static ThemeData get theme{
    return ThemeData(
      primaryColor: udiRed,
      //usamoes el color de la udi como base para el tema
      colorScheme: ColorScheme.fromSeed(seedColor: udiRed),

      //estilo global de los campos de texto 
      inputDecorationTheme: const InputDecorationTheme(
        //liena inferior blanca cuando el campo no esta selecionado
        enabledBorder: UnderlineInputBorder(
          borderSide: BorderSide(color: white)
        ),
        //linea inferior blanca cuando el campo esta selecionado
        focusedBorder: UnderlineInputBorder(
          borderSide: BorderSide(color: white, width: 2.0),
      ),
      //estilo del texto del "label" ejemplo: "nombre de usuario" en el login
      labelStyle: TextStyle(color: white,
      fontWeight: FontWeight.bold,
      fontSize: 14,
      ),
      //color de los iconos de la izquierda y la derecha
      prefixIconColor: white,
      suffixIconColor: white,
      ),

      //estilo global de los botones (ElevatedButton)
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          backgroundColor: white, //fondo del boton blanco para contrastar con el fondo rojo
          foregroundColor: udiRed, //color del texto del boton rojo para mantener la identidad visual
          textStyle: const TextStyle(
            fontWeight: FontWeight.bold, //texto en negrita para resaltar la importancia del botón
            fontSize: 16,
            letterSpacing: 1.2, //un poco de espacio entre letras para mejorar la legibilidad
          ),
          //bordes bien redondeados 
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(30.0),
          ),
          padding: const EdgeInsets.symmetric(vertical: 16
          ),
          ),

        ),
    );
  }
  
}