import 'package:flutter/material.dart';
import 'package:meu_barbeiro_core/meu_barbeiro_core.dart';

void main() {
  runApp(const MeuBarbeiroClienteApp());
}

class MeuBarbeiroClienteApp extends StatelessWidget {
  const MeuBarbeiroClienteApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'MeuBarbeiro Cliente',
      debugShowCheckedModeBanner: false,
      theme: MeuBarbeiroTheme.buildTheme(),
      home: const ClienteHomePage(),
    );
  }
}

class ClienteHomePage extends StatelessWidget {
  const ClienteHomePage({super.key});

  @override
  Widget build(BuildContext context) {
    final cards = <({IconData icon, String title, String subtitle})>[
      (
        icon: Icons.location_city_rounded,
        title: 'Buscar por cidade',
        subtitle: 'Listar barbearias disponiveis em uma cidade.'
      ),
      (
        icon: Icons.schedule_rounded,
        title: 'Escolher horario',
        subtitle: 'Consultar a disponibilidade antes de agendar.'
      ),
      (
        icon: Icons.star_rounded,
        title: 'Avaliar servico',
        subtitle: 'Registrar estrelas e comentario apos o atendimento.'
      ),
    ];

    return Scaffold(
      appBar: AppBar(
        title: const Text('MeuBarbeiro Cliente'),
      ),
      body: ListView(
        padding: const EdgeInsets.all(20),
        children: [
          const _HeroCard(
            title: 'Fluxo do cliente',
            subtitle:
                'Buscar uma barbearia, selecionar servicos, agendar e acompanhar o status.',
            badge: 'Sprint 3',
          ),
          const SizedBox(height: 20),
          for (final card in cards)
            Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: Card(
                child: ListTile(
                  leading: Icon(card.icon),
                  title: Text(card.title),
                  subtitle: Text(card.subtitle),
                ),
              ),
            ),
        ],
      ),
    );
  }
}

class _HeroCard extends StatelessWidget {
  const _HeroCard({
    required this.title,
    required this.subtitle,
    required this.badge,
  });

  final String title;
  final String subtitle;
  final String badge;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(24),
        gradient: const LinearGradient(
          colors: [Color(0xFF0E5A8A), Color(0xFF1F7AAE)],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
            decoration: BoxDecoration(
              color: Colors.white.withValues(alpha: 0.18),
              borderRadius: BorderRadius.circular(999),
            ),
            child: Text(
              badge,
              style: const TextStyle(color: Colors.white),
            ),
          ),
          const SizedBox(height: 16),
          Text(
            title,
            style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                  color: Colors.white,
                  fontWeight: FontWeight.w700,
                ),
          ),
          const SizedBox(height: 8),
          Text(
            subtitle,
            style: Theme.of(context).textTheme.bodyLarge?.copyWith(
                  color: Colors.white.withValues(alpha: 0.92),
                ),
          ),
        ],
      ),
    );
  }
}
