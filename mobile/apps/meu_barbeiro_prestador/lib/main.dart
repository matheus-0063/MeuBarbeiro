import 'package:flutter/material.dart';
import 'package:meu_barbeiro_core/meu_barbeiro_core.dart';

void main() {
  runApp(const MeuBarbeiroPrestadorApp());
}

class MeuBarbeiroPrestadorApp extends StatelessWidget {
  const MeuBarbeiroPrestadorApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'MeuBarbeiro Prestador',
      debugShowCheckedModeBanner: false,
      theme: MeuBarbeiroTheme.buildTheme(),
      home: const PrestadorHomePage(),
    );
  }
}

class PrestadorHomePage extends StatelessWidget {
  const PrestadorHomePage({super.key});

  @override
  Widget build(BuildContext context) {
    final cards = <({IconData icon, String title, String subtitle})>[
      (
        icon: Icons.notifications_active_rounded,
        title: 'Novas solicitacoes',
        subtitle: 'Receber notificacao assincrona quando um cliente agendar.'
      ),
      (
        icon: Icons.rule_folder_rounded,
        title: 'Aceitar ou recusar',
        subtitle: 'Atualizar o status com base na decisao do barbeiro.'
      ),
      (
        icon: Icons.manage_history_rounded,
        title: 'Acompanhar atendimentos',
        subtitle: 'Visualizar o historico e os servicos em andamento.'
      ),
    ];

    return Scaffold(
      appBar: AppBar(
        title: const Text('MeuBarbeiro Prestador'),
      ),
      body: ListView(
        padding: const EdgeInsets.all(20),
        children: [
          const _HeroCard(
            title: 'Fluxo do barbeiro',
            subtitle:
                'Receber a demanda, decidir sobre o atendimento e propagar a mudanca de status.',
            badge: 'Sprint 4',
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
          colors: [Color(0xFF123C52), Color(0xFF207868)],
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
              color: Colors.white.withValues(alpha: 0.16),
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
