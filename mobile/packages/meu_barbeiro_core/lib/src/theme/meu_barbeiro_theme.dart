import 'package:flutter/material.dart';

final class MeuBarbeiroTheme {
  static ThemeData buildTheme() {
    const seedColor = Color(0xFF0E5A8A);

    return ThemeData(
      colorScheme: ColorScheme.fromSeed(seedColor: seedColor),
      scaffoldBackgroundColor: const Color(0xFFF5F7FA),
      appBarTheme: const AppBarTheme(
        centerTitle: false,
        elevation: 0,
        backgroundColor: Color(0xFFF5F7FA),
        foregroundColor: Color(0xFF13212E),
      ),
      cardTheme: CardThemeData(
        color: Colors.white,
        elevation: 0,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(20),
          side: const BorderSide(color: Color(0xFFE2E8F0)),
        ),
      ),
    );
  }
}
