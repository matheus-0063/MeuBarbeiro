import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:meu_barbeiro_core/meu_barbeiro_core.dart';

import 'features/client_app_page.dart';

class MeuBarbeiroClienteApp extends StatelessWidget {
  const MeuBarbeiroClienteApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'MeuBarbeiro Cliente',
      debugShowCheckedModeBanner: false,
      theme: MeuBarbeiroTheme.buildTheme(),
      locale: const Locale('pt', 'BR'),
      supportedLocales: const [Locale('pt', 'BR')],
      localizationsDelegates: const [
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      home: const ClientAppPage(),
    );
  }
}
