import 'dart:convert';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:http/http.dart' as http;
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
      locale: const Locale('pt', 'BR'),
      supportedLocales: const [Locale('pt', 'BR')],
      localizationsDelegates: const [
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      home: const PrestadorFlowPage(),
    );
  }
}

class PrestadorFlowPage extends StatefulWidget {
  const PrestadorFlowPage({super.key});

  @override
  State<PrestadorFlowPage> createState() => _PrestadorFlowPageState();
}

class _PrestadorFlowPageState extends State<PrestadorFlowPage> {
  late final TextEditingController _apiBaseUrlController;
  final _loginEmailController = TextEditingController();
  final _loginPasswordController = TextEditingController();
  final _registerNameController = TextEditingController();
  final _registerEmailController = TextEditingController();
  final _registerPasswordController = TextEditingController();
  final _shopNameController = TextEditingController();
  final _shopCityController = TextEditingController();
  final _shopAddressController = TextEditingController();
  final _shopDescriptionController = TextEditingController();

  final http.Client _httpClient = http.Client();

  int _authTab = 0;
  bool _isBusy = false;
  PrestadorSession? _session;
  BarbershopDraft? _barbershop;
  FlowStep _step = FlowStep.auth;

  @override
  void initState() {
    super.initState();
    _apiBaseUrlController = TextEditingController(
      text: _resolveDefaultApiBaseUrl(),
    );
  }

  String _resolveDefaultApiBaseUrl() {
    const definedBaseUrl = String.fromEnvironment(
      'API_BASE_URL',
      defaultValue: '',
    );

    if (definedBaseUrl.isNotEmpty) {
      return definedBaseUrl;
    }

    if (!kIsWeb && defaultTargetPlatform == TargetPlatform.android) {
      return 'http://10.0.2.2:5039';
    }

    return 'http://localhost:5039';
  }

  @override
  void dispose() {
    _apiBaseUrlController.dispose();
    _loginEmailController.dispose();
    _loginPasswordController.dispose();
    _registerNameController.dispose();
    _registerEmailController.dispose();
    _registerPasswordController.dispose();
    _shopNameController.dispose();
    _shopCityController.dispose();
    _shopAddressController.dispose();
    _shopDescriptionController.dispose();
    _httpClient.close();
    super.dispose();
  }

  Future<void> _login() async {
    setState(() => _isBusy = true);

    try {
      final response = await _httpClient.post(
        Uri.parse('${_apiBaseUrlController.text.trim()}/api/v1/auth/login'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({
          'email': _loginEmailController.text.trim(),
          'password': _loginPasswordController.text,
        }),
      );

      final session = _parseSession(response);
      if (session.role != 'Barber') {
        throw Exception(
          'Use uma conta com perfil Barber para entrar no app do prestador.',
        );
      }

      if (!mounted) {
        return;
      }

      setState(() {
        _session = session;
        _step = FlowStep.dashboard;
      });
      _showMessage('Login realizado com sucesso.');
    } catch (error) {
      _showMessage(error.toString());
    } finally {
      if (mounted) {
        setState(() => _isBusy = false);
      }
    }
  }

  Future<void> _registerBarber() async {
    setState(() => _isBusy = true);

    try {
      final response = await _httpClient.post(
        Uri.parse(
          '${_apiBaseUrlController.text.trim()}/api/v1/auth/register/barber',
        ),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({
          'name': _registerNameController.text.trim(),
          'email': _registerEmailController.text.trim(),
          'password': _registerPasswordController.text,
          'barbershopId': null,
        }),
      );

      final session = _parseSession(response);

      if (!mounted) {
        return;
      }

      setState(() {
        _session = session;
        _step = FlowStep.barbershopForm;
      });
      _showMessage('Cadastro realizado. Agora preencha os dados da barbearia.');
    } catch (error) {
      _showMessage(error.toString());
    } finally {
      if (mounted) {
        setState(() => _isBusy = false);
      }
    }
  }

  Future<void> _saveBarbershop() async {
    final session = _session;
    if (session == null) {
      _showMessage('Sessao nao encontrada.');
      return;
    }

    setState(() => _isBusy = true);

    try {
      final response = await _httpClient.put(
        Uri.parse('${_apiBaseUrlController.text.trim()}/api/v1/barbershop/me'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ${session.accessToken}',
        },
        body: jsonEncode({
          'name': _shopNameController.text.trim(),
          'city': _shopCityController.text.trim(),
          'address': _shopAddressController.text.trim(),
          'description': _shopDescriptionController.text.trim(),
        }),
      );

      if (response.statusCode < 200 || response.statusCode >= 300) {
        throw Exception(_extractError(response));
      }

      final json = jsonDecode(response.body) as Map<String, dynamic>;

      if (!mounted) {
        return;
      }

      setState(() {
        _barbershop = BarbershopDraft.fromJson(json);
        _step = FlowStep.dashboard;
      });
      _showMessage('Barbearia salva com sucesso.');
    } catch (error) {
      _showMessage(error.toString());
    } finally {
      if (mounted) {
        setState(() => _isBusy = false);
      }
    }
  }

  PrestadorSession _parseSession(http.Response response) {
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception(_extractError(response));
    }

    final json = jsonDecode(response.body) as Map<String, dynamic>;
    return PrestadorSession.fromJson(json);
  }

  String _extractError(http.Response response) {
    try {
      final json = jsonDecode(response.body);
      if (json is Map<String, dynamic>) {
        final title = json['title'];
        if (title is String && title.isNotEmpty) {
          return title;
        }
      }
    } catch (_) {
      final text = response.body.trim();
      if (text.isNotEmpty) {
        return text;
      }
    }

    return 'Falha ao comunicar com a API (${response.statusCode}).';
  }

  void _showMessage(String message) {
    ScaffoldMessenger.of(
      context,
    ).showSnackBar(SnackBar(content: Text(message)));
  }

  @override
  Widget build(BuildContext context) {
    return switch (_step) {
      FlowStep.auth => _buildAuthScreen(context),
      FlowStep.barbershopForm => _buildBarbershopForm(context),
      FlowStep.dashboard => _buildDashboard(context),
    };
  }

  Widget _buildAuthScreen(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      body: SafeArea(
        child: ListView(
          padding: const EdgeInsets.fromLTRB(20, 12, 20, 24),
          children: [
            const _HeroBanner(
              eyebrow: 'Prestador',
              title: 'Entre ou crie sua conta de barbeiro.',
              subtitle:
                  'Quando o cadastro for concluido com sucesso, voce segue direto para preencher os dados da barbearia.',
            ),
            const SizedBox(height: 24),
            _SectionCard(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('API base URL', style: theme.textTheme.labelLarge),
                  const SizedBox(height: 8),
                  TextField(
                    controller: _apiBaseUrlController,
                    decoration: const InputDecoration(
                      hintText: 'http://10.0.2.2:5039',
                      prefixIcon: Icon(Icons.link_outlined),
                    ),
                  ),
                  const SizedBox(height: 20),
                  SegmentedButton<int>(
                    segments: const [
                      ButtonSegment(value: 0, label: Text('Login')),
                      ButtonSegment(value: 1, label: Text('Cadastro')),
                    ],
                    selected: {_authTab},
                    onSelectionChanged: (selection) {
                      setState(() => _authTab = selection.first);
                    },
                  ),
                  const SizedBox(height: 20),
                  if (_authTab == 0) ...[
                    TextField(
                      controller: _loginEmailController,
                      decoration: const InputDecoration(
                        hintText: 'E-mail',
                        prefixIcon: Icon(Icons.mail_outline),
                      ),
                    ),
                    const SizedBox(height: 12),
                    TextField(
                      controller: _loginPasswordController,
                      obscureText: true,
                      decoration: const InputDecoration(
                        hintText: 'Senha',
                        prefixIcon: Icon(Icons.lock_outline),
                      ),
                    ),
                    const SizedBox(height: 20),
                    FilledButton(
                      onPressed: _isBusy ? null : _login,
                      child: _isBusy
                          ? const SizedBox(
                              width: 18,
                              height: 18,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            )
                          : const Text('Entrar'),
                    ),
                  ] else ...[
                    TextField(
                      controller: _registerNameController,
                      decoration: const InputDecoration(
                        hintText: 'Nome completo',
                        prefixIcon: Icon(Icons.person_outline),
                      ),
                    ),
                    const SizedBox(height: 12),
                    TextField(
                      controller: _registerEmailController,
                      decoration: const InputDecoration(
                        hintText: 'E-mail',
                        prefixIcon: Icon(Icons.mail_outline),
                      ),
                    ),
                    const SizedBox(height: 12),
                    TextField(
                      controller: _registerPasswordController,
                      obscureText: true,
                      decoration: const InputDecoration(
                        hintText: 'Senha',
                        prefixIcon: Icon(Icons.lock_outline),
                      ),
                    ),
                    const SizedBox(height: 20),
                    FilledButton(
                      onPressed: _isBusy ? null : _registerBarber,
                      child: _isBusy
                          ? const SizedBox(
                              width: 18,
                              height: 18,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            )
                          : const Text('Criar conta de barbeiro'),
                    ),
                  ],
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildBarbershopForm(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      body: SafeArea(
        child: ListView(
          padding: const EdgeInsets.fromLTRB(20, 12, 20, 24),
          children: [
            const _HeroBanner(
              eyebrow: 'Barbearia',
              title: 'Complete os dados da sua barbearia.',
              subtitle:
                  'Esse passo acontece logo apos o cadastro do barbeiro ser concluido com sucesso.',
            ),
            const SizedBox(height: 24),
            _SectionCard(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('Nome da barbearia', style: theme.textTheme.labelLarge),
                  const SizedBox(height: 8),
                  TextField(controller: _shopNameController),
                  const SizedBox(height: 16),
                  Text('Cidade', style: theme.textTheme.labelLarge),
                  const SizedBox(height: 8),
                  TextField(controller: _shopCityController),
                  const SizedBox(height: 16),
                  Text('Endereco', style: theme.textTheme.labelLarge),
                  const SizedBox(height: 8),
                  TextField(controller: _shopAddressController),
                  const SizedBox(height: 16),
                  Text('Descricao', style: theme.textTheme.labelLarge),
                  const SizedBox(height: 8),
                  TextField(
                    controller: _shopDescriptionController,
                    minLines: 3,
                    maxLines: 5,
                  ),
                  const SizedBox(height: 20),
                  FilledButton(
                    onPressed: _isBusy ? null : _saveBarbershop,
                    child: _isBusy
                        ? const SizedBox(
                            width: 18,
                            height: 18,
                            child: CircularProgressIndicator(strokeWidth: 2),
                          )
                        : const Text('Salvar barbearia'),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildDashboard(BuildContext context) {
    final cards = <({IconData icon, String title, String subtitle})>[
      (
        icon: Icons.store_mall_directory_outlined,
        title: _barbershop?.name ?? 'Barbearia em configuracao',
        subtitle: _barbershop == null
            ? 'Complete os dados da barbearia para continuar.'
            : '${_barbershop!.city} • ${_barbershop!.address}',
      ),
      (
        icon: Icons.notifications_active_rounded,
        title: 'Novas solicitacoes',
        subtitle: 'Receber notificacao assincrona quando um cliente agendar.',
      ),
      (
        icon: Icons.rule_folder_rounded,
        title: 'Aceitar ou recusar',
        subtitle: 'Atualizar o status com base na decisao do barbeiro.',
      ),
    ];

    return Scaffold(
      appBar: AppBar(
        title: const Text('MeuBarbeiro Prestador'),
        actions: [
          TextButton(
            onPressed: () {
              setState(() => _step = FlowStep.barbershopForm);
            },
            child: const Text('Editar barbearia'),
          ),
        ],
      ),
      body: ListView(
        padding: const EdgeInsets.all(20),
        children: [
          _HeroBanner(
            eyebrow: _session?.name ?? 'Prestador',
            title: 'Fluxo do barbeiro pronto para continuar.',
            subtitle: _barbershop == null
                ? 'Seu cadastro foi concluido, mas ainda falta preencher a barbearia.'
                : 'Cadastro do barbeiro e dados da barbearia concluidos com sucesso.',
          ),
          const SizedBox(height: 20),
          for (final card in cards)
            Padding(
              padding: const EdgeInsets.only(bottom: 12),
              child: _SectionCard(
                child: ListTile(
                  contentPadding: EdgeInsets.zero,
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

enum FlowStep { auth, barbershopForm, dashboard }

class PrestadorSession {
  const PrestadorSession({
    required this.accessToken,
    required this.userId,
    required this.name,
    required this.email,
    required this.role,
  });

  factory PrestadorSession.fromJson(Map<String, dynamic> json) {
    return PrestadorSession(
      accessToken: json['accessToken'] as String,
      userId: json['userId'] as String,
      name: json['name'] as String,
      email: json['email'] as String,
      role: json['role'] as String,
    );
  }

  final String accessToken;
  final String userId;
  final String name;
  final String email;
  final String role;
}

class BarbershopDraft {
  const BarbershopDraft({
    required this.id,
    required this.name,
    required this.city,
    required this.address,
    required this.description,
  });

  factory BarbershopDraft.fromJson(Map<String, dynamic> json) {
    return BarbershopDraft(
      id: json['id'] as String,
      name: json['name'] as String,
      city: json['city'] as String,
      address: json['address'] as String,
      description: json['description'] as String,
    );
  }

  final String id;
  final String name;
  final String city;
  final String address;
  final String description;
}

class _HeroBanner extends StatelessWidget {
  const _HeroBanner({
    required this.eyebrow,
    required this.title,
    required this.subtitle,
  });

  final String eyebrow;
  final String title;
  final String subtitle;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Container(
      padding: const EdgeInsets.all(24),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(28),
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
              eyebrow,
              style: theme.textTheme.labelMedium?.copyWith(color: Colors.white),
            ),
          ),
          const SizedBox(height: 16),
          Text(
            title,
            style: theme.textTheme.headlineSmall?.copyWith(
              color: Colors.white,
              fontWeight: FontWeight.w700,
            ),
          ),
          const SizedBox(height: 8),
          Text(
            subtitle,
            style: theme.textTheme.bodyLarge?.copyWith(
              color: Colors.white.withValues(alpha: 0.92),
            ),
          ),
        ],
      ),
    );
  }
}

class _SectionCard extends StatelessWidget {
  const _SectionCard({required this.child});

  final Widget child;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(24),
        boxShadow: const [
          BoxShadow(
            color: Color(0x0A000000),
            blurRadius: 20,
            offset: Offset(0, 4),
          ),
        ],
      ),
      child: child,
    );
  }
}
