import 'dart:convert';
import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:http/http.dart' as http;
import 'package:intl/intl.dart';
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
  final _loginEmailController = TextEditingController();
  final _loginPasswordController = TextEditingController();
  final _registerNameController = TextEditingController();
  final _registerEmailController = TextEditingController();
  final _registerPasswordController = TextEditingController();
  final _shopNameController = TextEditingController();
  final _shopCityController = TextEditingController();
  final _shopAddressController = TextEditingController();
  final _shopDescriptionController = TextEditingController();
  final _serviceNameController = TextEditingController();
  final _servicePriceController = TextEditingController();
  final _serviceDurationController = TextEditingController();
  final _serviceDescriptionController = TextEditingController();
  late final String _apiBaseUrl;

  final http.Client _httpClient = http.Client();
  final NumberFormat _currency = NumberFormat.currency(
    locale: 'pt_BR',
    symbol: 'R\$',
  );

  int _authTab = 0;
  int _dashboardTab = 0;
  bool _isBusy = false;
  bool _isAppointmentsLoading = false;
  bool _isServicesLoading = false;
  bool _isServiceSubmitting = false;
  PrestadorSession? _session;
  BarbershopDraft? _barbershop;
  FlowStep _step = FlowStep.auth;
  BarberAppointmentStatusFilter? _statusFilter;
  String? _appointmentsError;
  String? _servicesError;
  List<BarberAppointment> _appointments = const [];
  List<BarbershopService> _services = const [];
  final Set<String> _updatingAppointmentIds = <String>{};
  Timer? _appointmentsPollingTimer;
  Set<String> _knownInAnalysisAppointmentIds = <String>{};
  int _newInAnalysisCount = 0;

  void _syncBarbershopControllers(BarbershopDraft? barbershop) {
    _shopNameController.text = barbershop?.name ?? '';
    _shopCityController.text = barbershop?.city ?? '';
    _shopAddressController.text = barbershop?.address ?? '';
    _shopDescriptionController.text = barbershop?.description ?? '';
  }

  @override
  void initState() {
    super.initState();
    _apiBaseUrl = _resolveDefaultApiBaseUrl();
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
    _appointmentsPollingTimer?.cancel();
    _loginEmailController.dispose();
    _loginPasswordController.dispose();
    _registerNameController.dispose();
    _registerEmailController.dispose();
    _registerPasswordController.dispose();
    _shopNameController.dispose();
    _shopCityController.dispose();
    _shopAddressController.dispose();
    _shopDescriptionController.dispose();
    _serviceNameController.dispose();
    _servicePriceController.dispose();
    _serviceDurationController.dispose();
    _serviceDescriptionController.dispose();
    _httpClient.close();
    super.dispose();
  }

  Future<void> _login() async {
    setState(() => _isBusy = true);

    try {
      final response = await _httpClient.post(
        Uri.parse('$_apiBaseUrl/api/v1/auth/login'),
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

      _session = session;
      await _loadMyBarbershopOrRedirect();
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
        Uri.parse('$_apiBaseUrl/api/v1/auth/register/barber'),
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
      _startAppointmentsPolling();
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
        Uri.parse('$_apiBaseUrl/api/v1/barbershop/me'),
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
      _syncBarbershopControllers(_barbershop);

      await _loadDashboardData();
      _startAppointmentsPolling();
      _showMessage('Barbearia salva com sucesso.');
    } catch (error) {
      _showMessage(error.toString());
    } finally {
      if (mounted) {
        setState(() => _isBusy = false);
      }
    }
  }

  Future<void> _loadMyBarbershopOrRedirect() async {
    final session = _session;
    if (session == null) {
      return;
    }

    final response = await _httpClient.get(
      Uri.parse('$_apiBaseUrl/api/v1/barbershop/me'),
      headers: {'Authorization': 'Bearer ${session.accessToken}'},
    );

    if (response.statusCode == 200) {
      final json = jsonDecode(response.body) as Map<String, dynamic>;
      if (!mounted) {
        return;
      }

      setState(() {
        _barbershop = BarbershopDraft.fromJson(json);
        _step = FlowStep.dashboard;
      });
      _syncBarbershopControllers(_barbershop);
      await _loadDashboardData();
      _startAppointmentsPolling();
      return;
    }

    if (response.statusCode == 404) {
      if (!mounted) {
        return;
      }

      setState(() {
        _barbershop = null;
        _step = FlowStep.barbershopForm;
      });
      _syncBarbershopControllers(null);
      return;
    }

    throw Exception(_extractError(response));
  }

  Future<void> _loadDashboardData() async {
    await Future.wait([_loadAppointments(), _loadServices()]);
  }

  Future<void> _loadAppointments() async {
    final session = _session;
    if (session == null) {
      return;
    }

    setState(() {
      _isAppointmentsLoading = true;
      _appointmentsError = null;
    });

    try {
      final uri = Uri.parse('$_apiBaseUrl/api/v1/appointment/mine').replace(
        queryParameters: {
          if (_statusFilter != null) 'status': _statusFilter!.apiValue,
        },
      );

      final response = await _httpClient.get(
        uri,
        headers: {'Authorization': 'Bearer ${session.accessToken}'},
      );

      if (response.statusCode != 200) {
        throw Exception(_extractError(response));
      }

      final decoded = jsonDecode(response.body) as List<dynamic>;

      if (!mounted) {
        return;
      }

      setState(() {
        final appointments = decoded
            .map(
              (item) =>
                  BarberAppointment.fromJson(item as Map<String, dynamic>),
            )
            .toList();
        _appointments = appointments;
        _updateInAnalysisNotifications(appointments);
        _isAppointmentsLoading = false;
      });
    } catch (error) {
      if (!mounted) {
        return;
      }

      setState(() {
        _appointmentsError = error.toString();
        _isAppointmentsLoading = false;
      });
    }
  }

  void _startAppointmentsPolling() {
    _appointmentsPollingTimer?.cancel();
    _appointmentsPollingTimer = Timer.periodic(const Duration(seconds: 8), (_) {
      if (_step == FlowStep.dashboard && _session != null && mounted) {
        _loadAppointments();
      }
    });
  }

  void _updateInAnalysisNotifications(List<BarberAppointment> appointments) {
    final currentIds = appointments
        .where((appointment) => appointment.status == 'InProgress')
        .map((appointment) => appointment.id)
        .toSet();

    if (_knownInAnalysisAppointmentIds.isNotEmpty) {
      final newIds = currentIds.difference(_knownInAnalysisAppointmentIds);
      if (newIds.isNotEmpty) {
        _newInAnalysisCount += newIds.length;
        WidgetsBinding.instance.addPostFrameCallback((_) {
          if (!mounted) {
            return;
          }

          _showMessage(
            newIds.length == 1
                ? 'Chegou 1 novo agendamento em analise.'
                : 'Chegaram ${newIds.length} novos agendamentos em analise.',
          );
        });
      }
    }

    _knownInAnalysisAppointmentIds = currentIds;
  }

  Future<void> _loadServices() async {
    final barbershop = _barbershop;
    if (barbershop == null) {
      return;
    }

    setState(() {
      _isServicesLoading = true;
      _servicesError = null;
    });

    try {
      final uri = Uri.parse(
        '$_apiBaseUrl/api/v1/services',
      ).replace(queryParameters: {'barbershopId': barbershop.id});

      final response = await _httpClient.get(uri);

      if (response.statusCode != 200) {
        throw Exception(_extractError(response));
      }

      final decoded = jsonDecode(response.body) as List<dynamic>;

      if (!mounted) {
        return;
      }

      setState(() {
        _services = decoded
            .map(
              (item) =>
                  BarbershopService.fromJson(item as Map<String, dynamic>),
            )
            .toList();
        _isServicesLoading = false;
      });
    } catch (error) {
      if (!mounted) {
        return;
      }

      setState(() {
        _servicesError = error.toString();
        _isServicesLoading = false;
      });
    }
  }

  Future<void> _createService() async {
    final session = _session;
    final barbershop = _barbershop;
    if (session == null || barbershop == null) {
      _showMessage('Barbearia ou sessao nao encontrada.');
      return;
    }

    final price = _parsePrice(_servicePriceController.text);
    final durationMinutes = int.tryParse(
      _serviceDurationController.text.trim(),
    );

    if (price == null) {
      _showMessage('Informe um preco valido. Ex.: 35 ou 35,90');
      return;
    }

    if (durationMinutes == null || durationMinutes <= 0) {
      _showMessage('Informe uma duracao valida em minutos.');
      return;
    }

    setState(() => _isServiceSubmitting = true);

    try {
      final response = await _httpClient.post(
        Uri.parse('$_apiBaseUrl/api/v1/services'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ${session.accessToken}',
        },
        body: jsonEncode({
          'barbershopId': barbershop.id,
          'name': _serviceNameController.text.trim(),
          'price': price,
          'description': _serviceDescriptionController.text.trim(),
          'durationMinutes': durationMinutes,
        }),
      );

      if (response.statusCode != 201) {
        throw Exception(_extractError(response));
      }

      _serviceNameController.clear();
      _servicePriceController.clear();
      _serviceDurationController.clear();
      _serviceDescriptionController.clear();

      if (!mounted) {
        return;
      }

      _showMessage('Servico cadastrado com sucesso.');
      await _loadServices();
    } catch (error) {
      _showMessage(error.toString());
    } finally {
      if (mounted) {
        setState(() => _isServiceSubmitting = false);
      }
    }
  }

  Future<void> _updateAppointmentStatus(
    BarberAppointment appointment,
    BarberAppointmentStatusUpdate status,
  ) async {
    final session = _session;
    if (session == null) {
      return;
    }

    setState(() => _updatingAppointmentIds.add(appointment.id));

    try {
      final response = await _httpClient.patch(
        Uri.parse('$_apiBaseUrl/api/v1/appointment/${appointment.id}/status'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ${session.accessToken}',
        },
        body: jsonEncode({'status': status.apiValue}),
      );

      if (response.statusCode != 204) {
        throw Exception(_extractError(response));
      }

      if (!mounted) {
        return;
      }

      _showMessage(
        status == BarberAppointmentStatusUpdate.accepted
            ? 'Pedido aceito com sucesso.'
            : 'Pedido recusado com sucesso.',
      );
      _newInAnalysisCount = 0;
      await _loadAppointments();
    } catch (error) {
      _showMessage(error.toString());
    } finally {
      if (mounted) {
        setState(() => _updatingAppointmentIds.remove(appointment.id));
      }
    }
  }

  void _logout() {
    _appointmentsPollingTimer?.cancel();
    setState(() {
      _session = null;
      _barbershop = null;
      _appointments = const [];
      _services = const [];
      _appointmentsError = null;
      _servicesError = null;
      _step = FlowStep.auth;
      _dashboardTab = 0;
      _statusFilter = null;
      _updatingAppointmentIds.clear();
      _knownInAnalysisAppointmentIds = <String>{};
      _newInAnalysisCount = 0;
    });
    _syncBarbershopControllers(null);
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
        final errors = json['errors'];
        if (errors is Map<String, dynamic>) {
          final flattened = <String>[];
          for (final value in errors.values) {
            if (value is List) {
              flattened.addAll(value.map((item) => item.toString()));
            }
          }

          if (flattened.isNotEmpty) {
            return flattened.join('\n');
          }
        }

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

  double? _parsePrice(String rawValue) {
    final trimmed = rawValue.trim();
    final normalized = trimmed.contains(',')
        ? trimmed.replaceAll('.', '').replaceAll(',', '.')
        : trimmed;
    return double.tryParse(normalized);
  }

  String _statusLabel(String status) {
    switch (status) {
      case 'Accepted':
        return 'Aceito';
      case 'Rejected':
        return 'Recusado';
      case 'InProgress':
        return 'Em analise';
      case 'Completed':
        return 'Concluido';
      case 'Cancelled':
        return 'Cancelado';
      default:
        return 'Pendente';
    }
  }

  Color _statusBackground(String status) {
    switch (status) {
      case 'Accepted':
        return const Color(0xFFDFF4E8);
      case 'Rejected':
        return const Color(0xFFFFE2DE);
      case 'InProgress':
        return const Color(0xFFFFF3D9);
      case 'Completed':
        return const Color(0xFFE3ECFF);
      case 'Cancelled':
        return const Color(0xFFE9EAEB);
      default:
        return const Color(0xFFF3F4F5);
    }
  }

  Color _statusForeground(String status) {
    switch (status) {
      case 'Accepted':
        return const Color(0xFF1E6B43);
      case 'Rejected':
        return const Color(0xFF8B2E23);
      case 'InProgress':
        return const Color(0xFF8A5A00);
      case 'Completed':
        return const Color(0xFF274F9F);
      case 'Cancelled':
        return const Color(0xFF5A5F64);
      default:
        return const Color(0xFF414846);
    }
  }

  String _dateLabel(DateTime dateTime) {
    return DateFormat(
      "dd 'de' MMMM • HH:mm",
      'pt_BR',
    ).format(dateTime.toLocal());
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
    return Scaffold(
      body: SafeArea(
        child: ListView(
          padding: const EdgeInsets.fromLTRB(20, 12, 20, 24),
          children: [
            const _HeroBanner(
              eyebrow: 'Prestador',
              title: 'Entre ou crie sua conta de barbeiro.',
              subtitle:
                  'Depois do cadastro, voce segue para completar os dados da barbearia e acessar os pedidos recebidos.',
            ),
            const SizedBox(height: 24),
            _SectionCard(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
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
                  'Esse passo e necessario antes de receber pedidos e decidir sobre aceite ou recusa.',
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
    final pages = [
      _buildOrdersPage(context),
      _buildServicesPage(context),
      _buildProfilePage(context),
    ];

    return Scaffold(
      body: SafeArea(
        child: IndexedStack(index: _dashboardTab, children: pages),
      ),
      bottomNavigationBar: NavigationBar(
        selectedIndex: _dashboardTab,
        onDestinationSelected: (index) => setState(() => _dashboardTab = index),
        destinations: const [
          NavigationDestination(
            icon: Icon(Icons.receipt_long_outlined),
            selectedIcon: Icon(Icons.receipt_long),
            label: 'Pedidos',
          ),
          NavigationDestination(
            icon: Icon(Icons.content_cut_outlined),
            label: 'Servicos',
          ),
          NavigationDestination(
            icon: Icon(Icons.storefront_outlined),
            label: 'Perfil',
          ),
        ],
      ),
    );
  }

  Widget _buildOrdersPage(BuildContext context) {
    final theme = Theme.of(context);

    return RefreshIndicator(
      onRefresh: _loadAppointments,
      child: ListView(
        padding: const EdgeInsets.fromLTRB(20, 12, 20, 24),
        children: [
          _HeroBanner(
            eyebrow: _barbershop?.name ?? 'Pedidos',
            title: 'Acompanhe os pedidos recebidos e decida o aceite no app.',
            subtitle:
                'Veja os pedidos recebidos, filtre por status e responda quando quiser.',
          ),
          if (_newInAnalysisCount > 0) ...[
            const SizedBox(height: 16),
            _SectionCard(
              child: Row(
                children: [
                  const Icon(Icons.notifications_active_outlined),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Text(
                      _newInAnalysisCount == 1
                          ? 'Voce tem 1 novo agendamento em analise.'
                          : 'Voce tem $_newInAnalysisCount novos agendamentos em analise.',
                      style: theme.textTheme.bodyLarge,
                    ),
                  ),
                  TextButton(
                    onPressed: () {
                      setState(() => _newInAnalysisCount = 0);
                    },
                    child: const Text('Dispensar'),
                  ),
                ],
              ),
            ),
          ],
          const SizedBox(height: 24),
          SingleChildScrollView(
            scrollDirection: Axis.horizontal,
            child: Row(
              children: [
                Padding(
                  padding: const EdgeInsets.only(right: 8),
                  child: ChoiceChip(
                    label: const Text('Todos'),
                    selected: _statusFilter == null,
                    onSelected: (_) async {
                      setState(() => _statusFilter = null);
                      await _loadAppointments();
                    },
                  ),
                ),
                for (final filter in BarberAppointmentStatusFilter.values)
                  Padding(
                    padding: const EdgeInsets.only(right: 8),
                    child: ChoiceChip(
                      label: Text(filter.label),
                      selected: _statusFilter == filter,
                      onSelected: (_) async {
                        setState(() => _statusFilter = filter);
                        await _loadAppointments();
                      },
                    ),
                  ),
              ],
            ),
          ),
          const SizedBox(height: 20),
          if (_isAppointmentsLoading)
            const Padding(
              padding: EdgeInsets.symmetric(vertical: 32),
              child: Center(child: CircularProgressIndicator()),
            )
          else if (_appointmentsError != null)
            _SectionCard(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Nao foi possivel carregar os pedidos.',
                    style: theme.textTheme.titleMedium,
                  ),
                  const SizedBox(height: 8),
                  Text(_appointmentsError!, style: theme.textTheme.bodyMedium),
                ],
              ),
            )
          else if (_appointments.isEmpty)
            _SectionCard(
              child: Text(
                'Nenhum pedido encontrado para os filtros atuais.',
                style: theme.textTheme.bodyMedium,
              ),
            )
          else
            for (final appointment in _appointments)
              Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: _SectionCard(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: [
                          Expanded(
                            child: Text(
                              _dateLabel(appointment.scheduledAtUtc),
                              style: theme.textTheme.titleMedium,
                            ),
                          ),
                          _StatusPill(
                            label: _statusLabel(appointment.status),
                            background: _statusBackground(appointment.status),
                            foreground: _statusForeground(appointment.status),
                          ),
                        ],
                      ),
                      const SizedBox(height: 10),
                      Text(
                        'Valor do pedido: ${_currency.format(appointment.totalPrice)}',
                        style: theme.textTheme.bodyLarge,
                      ),
                      const SizedBox(height: 6),
                      Text(
                        'Cliente: ${appointment.clientName}',
                        style: theme.textTheme.bodySmall,
                      ),
                      const SizedBox(height: 6),
                      Text(
                        'Barbearia: ${appointment.barbershopName}',
                        style: theme.textTheme.bodySmall,
                      ),
                      if (appointment.selectedServices.isNotEmpty) ...[
                        const SizedBox(height: 6),
                        Text(
                          'Servicos: ${appointment.selectedServices.map((service) => service.name).join(', ')}',
                          style: theme.textTheme.bodySmall,
                        ),
                      ],
                      if (appointment.status == 'Pending' ||
                          appointment.status == 'InProgress') ...[
                        const SizedBox(height: 16),
                        Row(
                          children: [
                            Expanded(
                              child: FilledButton(
                                onPressed:
                                    _updatingAppointmentIds.contains(
                                      appointment.id,
                                    )
                                    ? null
                                    : () => _updateAppointmentStatus(
                                        appointment,
                                        BarberAppointmentStatusUpdate.accepted,
                                      ),
                                child:
                                    _updatingAppointmentIds.contains(
                                      appointment.id,
                                    )
                                    ? const SizedBox(
                                        width: 18,
                                        height: 18,
                                        child: CircularProgressIndicator(
                                          strokeWidth: 2,
                                        ),
                                      )
                                    : const Text('Aceitar'),
                              ),
                            ),
                            const SizedBox(width: 12),
                            Expanded(
                              child: OutlinedButton(
                                onPressed:
                                    _updatingAppointmentIds.contains(
                                      appointment.id,
                                    )
                                    ? null
                                    : () => _updateAppointmentStatus(
                                        appointment,
                                        BarberAppointmentStatusUpdate.rejected,
                                      ),
                                child: const Text('Recusar'),
                              ),
                            ),
                          ],
                        ),
                      ],
                    ],
                  ),
                ),
              ),
        ],
      ),
    );
  }

  Widget _buildProfilePage(BuildContext context) {
    final theme = Theme.of(context);

    return ListView(
      padding: const EdgeInsets.fromLTRB(20, 12, 20, 24),
      children: [
        _HeroBanner(
          eyebrow: _session?.name ?? 'Prestador',
          title: 'Perfil do barbeiro e dados da barbearia.',
          subtitle:
              'Revise os dados da sua conta, ajuste a barbearia e saia da sessao.',
        ),
        const SizedBox(height: 24),
        _SectionCard(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(_session?.name ?? '', style: theme.textTheme.titleMedium),
              const SizedBox(height: 4),
              Text(_session?.email ?? '', style: theme.textTheme.bodyMedium),
              if (_barbershop != null) ...[
                const SizedBox(height: 16),
                Text(_barbershop!.name, style: theme.textTheme.titleMedium),
                const SizedBox(height: 4),
                Text(
                  '${_barbershop!.city} • ${_barbershop!.address}',
                  style: theme.textTheme.bodyMedium,
                ),
                const SizedBox(height: 4),
                Text(
                  _barbershop!.description,
                  style: theme.textTheme.bodySmall,
                ),
              ],
              const SizedBox(height: 20),
              Wrap(
                spacing: 12,
                runSpacing: 12,
                children: [
                  FilledButton(
                    onPressed: () {
                      _syncBarbershopControllers(_barbershop);
                      setState(() => _step = FlowStep.barbershopForm);
                    },
                    child: const Text('Editar barbearia'),
                  ),
                  OutlinedButton(
                    onPressed: _loadDashboardData,
                    child: const Text('Atualizar dados'),
                  ),
                  OutlinedButton(onPressed: _logout, child: const Text('Sair')),
                ],
              ),
            ],
          ),
        ),
      ],
    );
  }

  Widget _buildServicesPage(BuildContext context) {
    final theme = Theme.of(context);

    return RefreshIndicator(
      onRefresh: _loadServices,
      child: ListView(
        padding: const EdgeInsets.fromLTRB(20, 12, 20, 24),
        children: [
          _HeroBanner(
            eyebrow: _barbershop?.name ?? 'Servicos',
            title: 'Cadastre e acompanhe os servicos da sua barbearia.',
            subtitle:
                'Adicione novos servicos com preco, duracao e descricao para aparecerem no app do cliente.',
          ),
          const SizedBox(height: 24),
          _SectionCard(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('Novo servico', style: theme.textTheme.titleMedium),
                const SizedBox(height: 16),
                TextField(
                  controller: _serviceNameController,
                  decoration: const InputDecoration(
                    hintText: 'Ex.: Corte degradê',
                    prefixIcon: Icon(Icons.design_services_outlined),
                  ),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: _servicePriceController,
                  keyboardType: const TextInputType.numberWithOptions(
                    decimal: true,
                  ),
                  decoration: const InputDecoration(
                    hintText: 'Preco ex.: 35,90',
                    prefixIcon: Icon(Icons.attach_money_outlined),
                  ),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: _serviceDurationController,
                  keyboardType: TextInputType.number,
                  decoration: const InputDecoration(
                    hintText: 'Duracao em minutos',
                    prefixIcon: Icon(Icons.schedule_outlined),
                  ),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: _serviceDescriptionController,
                  minLines: 3,
                  maxLines: 5,
                  decoration: const InputDecoration(
                    hintText: 'Descreva o que esta incluso no servico',
                    prefixIcon: Icon(Icons.notes_outlined),
                  ),
                ),
                const SizedBox(height: 20),
                FilledButton(
                  onPressed: _isServiceSubmitting ? null : _createService,
                  child: _isServiceSubmitting
                      ? const SizedBox(
                          width: 18,
                          height: 18,
                          child: CircularProgressIndicator(strokeWidth: 2),
                        )
                      : const Text('Cadastrar servico'),
                ),
              ],
            ),
          ),
          const SizedBox(height: 24),
          Text('Servicos cadastrados', style: theme.textTheme.titleLarge),
          const SizedBox(height: 12),
          if (_isServicesLoading)
            const Padding(
              padding: EdgeInsets.symmetric(vertical: 32),
              child: Center(child: CircularProgressIndicator()),
            )
          else if (_servicesError != null)
            _SectionCard(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Nao foi possivel carregar os servicos.',
                    style: theme.textTheme.titleMedium,
                  ),
                  const SizedBox(height: 8),
                  Text(_servicesError!, style: theme.textTheme.bodyMedium),
                ],
              ),
            )
          else if (_services.isEmpty)
            _SectionCard(
              child: Text(
                'Nenhum servico cadastrado ainda.',
                style: theme.textTheme.bodyMedium,
              ),
            )
          else
            for (final service in _services)
              Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: _SectionCard(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(service.name, style: theme.textTheme.titleMedium),
                      const SizedBox(height: 8),
                      Text(
                        _currency.format(service.price),
                        style: theme.textTheme.bodyLarge,
                      ),
                      const SizedBox(height: 6),
                      Text(
                        '${service.durationMinutes} min',
                        style: theme.textTheme.labelLarge,
                      ),
                      const SizedBox(height: 10),
                      Text(
                        service.description,
                        style: theme.textTheme.bodyMedium,
                      ),
                    ],
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

class BarberAppointment {
  const BarberAppointment({
    required this.id,
    required this.clientId,
    required this.clientName,
    required this.barberId,
    required this.barbershopId,
    required this.barbershopName,
    required this.scheduledAtUtc,
    required this.totalPrice,
    required this.status,
    required this.selectedServices,
  });

  factory BarberAppointment.fromJson(Map<String, dynamic> json) {
    return BarberAppointment(
      id: json['id'] as String,
      clientId: json['clientId'] as String,
      clientName: (json['clientName'] as String?) ?? '',
      barberId: json['barberId'] as String,
      barbershopId: json['barbershopId'] as String,
      barbershopName: (json['barbershopName'] as String?) ?? '',
      scheduledAtUtc: DateTime.parse(json['scheduledAtUtc'] as String),
      totalPrice: (json['totalPrice'] as num).toDouble(),
      status: json['status'] as String,
      selectedServices: ((json['selectedServices'] as List<dynamic>?) ?? const [])
          .map((item) => AppointmentSelectedService.fromJson(item as Map<String, dynamic>))
          .toList(),
    );
  }

  final String id;
  final String clientId;
  final String clientName;
  final String barberId;
  final String barbershopId;
  final String barbershopName;
  final DateTime scheduledAtUtc;
  final double totalPrice;
  final String status;
  final List<AppointmentSelectedService> selectedServices;
}

class AppointmentSelectedService {
  const AppointmentSelectedService({
    required this.serviceId,
    required this.name,
    required this.price,
    required this.durationMinutes,
  });

  factory AppointmentSelectedService.fromJson(Map<String, dynamic> json) {
    return AppointmentSelectedService(
      serviceId: json['serviceId'] as String,
      name: json['name'] as String,
      price: (json['price'] as num).toDouble(),
      durationMinutes: json['durationMinutes'] as int,
    );
  }

  final String serviceId;
  final String name;
  final double price;
  final int durationMinutes;
}

class BarbershopService {
  const BarbershopService({
    required this.id,
    required this.barbershopId,
    required this.name,
    required this.price,
    required this.description,
    required this.durationMinutes,
  });

  factory BarbershopService.fromJson(Map<String, dynamic> json) {
    return BarbershopService(
      id: json['id'] as String,
      barbershopId: json['barbershopId'] as String,
      name: json['name'] as String,
      price: (json['price'] as num).toDouble(),
      description: json['description'] as String,
      durationMinutes: json['durationMinutes'] as int,
    );
  }

  final String id;
  final String barbershopId;
  final String name;
  final double price;
  final String description;
  final int durationMinutes;
}

enum BarberAppointmentStatusFilter {
  pending('Pending', 'Pendentes'),
  accepted('Accepted', 'Aceitos'),
  rejected('Rejected', 'Recusados'),
  inProgress('InProgress', 'Em analise'),
  completed('Completed', 'Concluidos'),
  cancelled('Cancelled', 'Cancelados');

  const BarberAppointmentStatusFilter(this.apiValue, this.label);

  final String apiValue;
  final String label;
}

enum BarberAppointmentStatusUpdate {
  accepted(1),
  rejected(2);

  const BarberAppointmentStatusUpdate(this.apiValue);

  final int apiValue;
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

class _StatusPill extends StatelessWidget {
  const _StatusPill({
    required this.label,
    required this.background,
    required this.foreground,
  });

  final String label;
  final Color background;
  final Color foreground;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      decoration: BoxDecoration(
        color: background,
        borderRadius: BorderRadius.circular(999),
      ),
      child: Text(
        label,
        style: Theme.of(context).textTheme.labelMedium?.copyWith(
          color: foreground,
          fontWeight: FontWeight.w700,
        ),
      ),
    );
  }
}
