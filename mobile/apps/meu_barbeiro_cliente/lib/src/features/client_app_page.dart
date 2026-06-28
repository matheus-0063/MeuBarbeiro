import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../data/backend_api.dart';
import '../data/local_store.dart';

class ClientAppPage extends StatefulWidget {
  const ClientAppPage({super.key});

  @override
  State<ClientAppPage> createState() => _ClientAppPageState();
}

class _ClientAppPageState extends State<ClientAppPage> {
  final BackendApi _api = BackendApi();
  final LocalStore _localStore = LocalStore();

  late final TextEditingController _loginEmailController;
  late final TextEditingController _loginPasswordController;
  late final TextEditingController _registerNameController;
  late final TextEditingController _registerEmailController;
  late final TextEditingController _registerPasswordController;
  late final TextEditingController _cityController;
  late final String _apiBaseUrl;

  AuthSession? _session;
  bool _isBootstrapping = true;
  bool _isAuthBusy = false;
  bool _isShopsLoading = false;
  bool _isAppointmentsLoading = false;
  int _currentTab = 0;
  int _authTab = 0;
  String? _shopsError;
  String? _appointmentsError;
  List<BarbershopSummary> _barbershops = const [];
  List<ClientAppointment> _appointments = const [];
  final Map<String, BarbershopSummary> _barbershopCache = {};
  AppointmentStatusFilter? _statusFilter;

  @override
  void initState() {
    super.initState();
    _apiBaseUrl = _resolveDefaultApiBaseUrl();
    _loginEmailController = TextEditingController();
    _loginPasswordController = TextEditingController();
    _registerNameController = TextEditingController();
    _registerEmailController = TextEditingController();
    _registerPasswordController = TextEditingController();
    _cityController = TextEditingController(text: 'Sao Paulo');
    _bootstrap();
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
    _loginEmailController.dispose();
    _loginPasswordController.dispose();
    _registerNameController.dispose();
    _registerEmailController.dispose();
    _registerPasswordController.dispose();
    _cityController.dispose();
    super.dispose();
  }

  Future<void> _bootstrap() async {
    final storedSession = await _localStore.loadSession();

    if (!mounted) {
      return;
    }

    setState(() {
      _session = storedSession;
      _isBootstrapping = false;
    });

    if (storedSession != null && storedSession.role == 'Client') {
      await _loadInitialClientData();
    }
  }

  Future<void> _loadInitialClientData() async {
    await Future.wait([_loadBarbershops(), _loadAppointments()]);
  }

  Future<void> _setSession(AuthSession session) async {
    await _localStore.saveSession(session);

    if (!mounted) {
      return;
    }

    setState(() {
      _session = session;
      _currentTab = 0;
    });

    await _loadInitialClientData();
  }

  Future<void> _logout() async {
    await _localStore.clearSession();
    if (!mounted) {
      return;
    }

    setState(() {
      _session = null;
      _appointments = const [];
      _barbershops = const [];
    });
  }

  Future<void> _submitLogin() async {
    setState(() => _isAuthBusy = true);

    try {
      final session = await _api.login(
        baseUrl: _apiBaseUrl,
        email: _loginEmailController.text.trim(),
        password: _loginPasswordController.text,
      );

      if (session.role != 'Client') {
        throw BackendApiException(
          'Este app aceita apenas autenticacao de clientes.',
        );
      }

      await _setSession(session);
      _showMessage('Login realizado com sucesso.');
    } catch (error) {
      _showMessage(error.toString());
    } finally {
      if (mounted) {
        setState(() => _isAuthBusy = false);
      }
    }
  }

  Future<void> _submitRegister() async {
    setState(() => _isAuthBusy = true);

    try {
      final session = await _api.registerClient(
        baseUrl: _apiBaseUrl,
        name: _registerNameController.text.trim(),
        email: _registerEmailController.text.trim(),
        password: _registerPasswordController.text,
      );

      await _setSession(session);
      _showMessage('Cadastro realizado com sucesso.');
    } catch (error) {
      _showMessage(error.toString());
    } finally {
      if (mounted) {
        setState(() => _isAuthBusy = false);
      }
    }
  }

  Future<void> _loadBarbershops() async {
    setState(() {
      _isShopsLoading = true;
      _shopsError = null;
    });

    try {
      final shops = await _api.listBarbershops(
        baseUrl: _apiBaseUrl,
        city: _cityController.text.trim(),
      );

      if (!mounted) {
        return;
      }

      setState(() {
        _barbershops = shops;
        _isShopsLoading = false;
        for (final shop in shops) {
          _barbershopCache[shop.id] = shop;
        }
      });
    } catch (error) {
      if (!mounted) {
        return;
      }

      setState(() {
        _shopsError = error.toString();
        _isShopsLoading = false;
      });
    }
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
      final appointments = await _api.getMyAppointments(
        baseUrl: _apiBaseUrl,
        accessToken: session.accessToken,
        status: _statusFilter,
      );

      if (!mounted) {
        return;
      }

      setState(() {
        _appointments = appointments;
        _isAppointmentsLoading = false;
      });

      for (final appointment in appointments) {
        if (appointment.barbershopName.isNotEmpty &&
            !_barbershopCache.containsKey(appointment.barbershopId)) {
          _barbershopCache[appointment.barbershopId] = BarbershopSummary(
            id: appointment.barbershopId,
            name: appointment.barbershopName,
            city: '',
            address: '',
            description: '',
            averageRating: 0,
          );
        }

        if (!_barbershopCache.containsKey(appointment.barbershopId)) {
          try {
            final shop = await _api.getBarbershop(
              baseUrl: _apiBaseUrl,
              barbershopId: appointment.barbershopId,
            );
            _barbershopCache[shop.id] = shop;
          } catch (_) {
            // Mantem fallback visual com o id se a API falhar.
          }
        }
      }

      if (mounted) {
        setState(() {});
      }
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

  Future<void> _openBookingScreen(BarbershopSummary shop) async {
    final session = _session;
    if (session == null) {
      return;
    }

    final created = await Navigator.of(context).push<bool>(
      MaterialPageRoute(
        builder: (context) => BarbershopBookingPage(
          api: _api,
          baseUrl: _apiBaseUrl,
          session: session,
          shop: shop,
        ),
      ),
    );

    if (created == true && mounted) {
      setState(() => _currentTab = 1);
      await _loadAppointments();
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_isBootstrapping) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }

    if (_session == null) {
      return _AuthScreen(
        authTab: _authTab,
        loginEmailController: _loginEmailController,
        loginPasswordController: _loginPasswordController,
        registerNameController: _registerNameController,
        registerEmailController: _registerEmailController,
        registerPasswordController: _registerPasswordController,
        isBusy: _isAuthBusy,
        onSwitchTab: (index) => setState(() => _authTab = index),
        onLogin: _submitLogin,
        onRegister: _submitRegister,
      );
    }

    if (_session!.role != 'Client') {
      return Scaffold(
        body: SafeArea(
          child: Padding(
            padding: const EdgeInsets.all(20),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const _HeroBanner(
                  eyebrow: 'Perfil invalido',
                  title:
                      'Este aplicativo foi preparado para o fluxo do cliente.',
                  subtitle:
                      'Use um login com role Client para navegar pelas telas e agendar servicos.',
                ),
                const SizedBox(height: 20),
                FilledButton(onPressed: _logout, child: const Text('Sair')),
              ],
            ),
          ),
        ),
      );
    }

    return Scaffold(
      body: SafeArea(
        child: IndexedStack(
          index: _currentTab,
          children: [
            _buildDiscoverPage(context),
            _buildAppointmentsPage(context),
            _buildAccountPage(context),
          ],
        ),
      ),
      bottomNavigationBar: NavigationBar(
        selectedIndex: _currentTab,
        onDestinationSelected: (index) => setState(() => _currentTab = index),
        destinations: const [
          NavigationDestination(
            icon: Icon(Icons.storefront_outlined),
            label: 'Explorar',
          ),
          NavigationDestination(
            icon: Icon(Icons.event_note_outlined),
            label: 'Agenda',
          ),
          NavigationDestination(
            icon: Icon(Icons.person_outline),
            label: 'Conta',
          ),
        ],
      ),
    );
  }

  Widget _buildDiscoverPage(BuildContext context) {
    final theme = Theme.of(context);

    return RefreshIndicator(
      onRefresh: _loadBarbershops,
      child: ListView(
        padding: const EdgeInsets.fromLTRB(20, 12, 20, 24),
        children: [
          const _HeroBanner(
            eyebrow: 'Cliente',
            title:
                'Busque uma barbearia e siga para uma tela dedicada de agendamento.',
            subtitle:
                'Ao tocar em uma barbearia, voce sera redirecionado para a tela com servicos, dias, horarios e o botao de solicitar.',
          ),
          const SizedBox(height: 24),
          Text('Cidade', style: theme.textTheme.labelLarge),
          const SizedBox(height: 8),
          Row(
            children: [
              Expanded(
                child: TextField(
                  controller: _cityController,
                  decoration: const InputDecoration(
                    hintText: 'Ex.: Sao Paulo',
                    prefixIcon: Icon(Icons.location_on_outlined),
                  ),
                ),
              ),
              const SizedBox(width: 12),
              FilledButton(
                onPressed: _isShopsLoading ? null : _loadBarbershops,
                child: const Text('Buscar'),
              ),
            ],
          ),
          const SizedBox(height: 24),
          Text('Barbearias', style: theme.textTheme.titleLarge),
          const SizedBox(height: 12),
          if (_isShopsLoading)
            const Padding(
              padding: EdgeInsets.symmetric(vertical: 32),
              child: Center(child: CircularProgressIndicator()),
            )
          else if (_shopsError != null)
            _SectionCard(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Nao foi possivel carregar as barbearias.',
                    style: theme.textTheme.titleMedium,
                  ),
                  const SizedBox(height: 8),
                  Text(_shopsError!, style: theme.textTheme.bodyMedium),
                ],
              ),
            )
          else if (_barbershops.isEmpty)
            _SectionCard(
              child: Text(
                'Nenhuma barbearia encontrada para a cidade informada.',
                style: theme.textTheme.bodyMedium,
              ),
            )
          else
            for (final shop in _barbershops)
              Padding(
                padding: const EdgeInsets.only(bottom: 12),
                child: _ShopCard(
                  shop: shop,
                  selected: false,
                  onTap: () => _openBookingScreen(shop),
                ),
              ),
        ],
      ),
    );
  }

  Widget _buildAppointmentsPage(BuildContext context) {
    final theme = Theme.of(context);

    return RefreshIndicator(
      onRefresh: _loadAppointments,
      child: ListView(
        padding: const EdgeInsets.fromLTRB(20, 12, 20, 24),
        children: [
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
                for (final filter in AppointmentStatusFilter.values)
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
                    'Nao foi possivel carregar a agenda.',
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
                'Nenhum agendamento encontrado para os filtros atuais.',
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
                              appointment.barbershopName.isNotEmpty
                                  ? appointment.barbershopName
                                  : _barbershopCache[appointment.barbershopId]
                                        ?.name ??
                                  'Barbearia ${appointment.barbershopId.substring(0, 8)}',
                              style: theme.textTheme.titleMedium,
                            ),
                          ),
                          Container(
                            padding: const EdgeInsets.symmetric(
                              horizontal: 12,
                              vertical: 8,
                            ),
                            decoration: BoxDecoration(
                              color: _statusBackground(appointment.status),
                              borderRadius: BorderRadius.circular(999),
                            ),
                            child: Text(
                              _statusLabel(appointment.status),
                              style: theme.textTheme.labelMedium?.copyWith(
                                color: _statusForeground(appointment.status),
                                fontWeight: FontWeight.w700,
                              ),
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 10),
                      Text(
                        _dateLabel(appointment.scheduledAtUtc),
                        style: theme.textTheme.bodyLarge,
                      ),
                      if (appointment.selectedServices.isNotEmpty) ...[
                        const SizedBox(height: 8),
                        Text(
                          appointment.selectedServices
                              .map((service) => service.name)
                              .join(' • '),
                          style: theme.textTheme.bodyMedium,
                        ),
                      ],
                      const SizedBox(height: 6),
                      Text(
                        NumberFormat.currency(
                          locale: 'pt_BR',
                          symbol: 'R\$',
                        ).format(appointment.totalPrice),
                        style: theme.textTheme.labelLarge,
                      ),
                    ],
                  ),
                ),
              ),
        ],
      ),
    );
  }

  Widget _buildAccountPage(BuildContext context) {
    final theme = Theme.of(context);

    return ListView(
      padding: const EdgeInsets.fromLTRB(20, 12, 20, 24),
      children: [
        _HeroBanner(
          eyebrow: 'Conta',
          title: 'Seus dados e sua conta.',
          subtitle: 'Acompanhe seus acessos e saia da conta quando quiser.',
        ),
        const SizedBox(height: 24),
        _SectionCard(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(_session!.name, style: theme.textTheme.titleMedium),
              const SizedBox(height: 4),
              Text(_session!.email, style: theme.textTheme.bodyMedium),
              const SizedBox(height: 20),
              FilledButton(onPressed: _logout, child: const Text('Sair')),
            ],
          ),
        ),
      ],
    );
  }
}

class _AuthScreen extends StatelessWidget {
  const _AuthScreen({
    required this.authTab,
    required this.loginEmailController,
    required this.loginPasswordController,
    required this.registerNameController,
    required this.registerEmailController,
    required this.registerPasswordController,
    required this.isBusy,
    required this.onSwitchTab,
    required this.onLogin,
    required this.onRegister,
  });

  final int authTab;
  final TextEditingController loginEmailController;
  final TextEditingController loginPasswordController;
  final TextEditingController registerNameController;
  final TextEditingController registerEmailController;
  final TextEditingController registerPasswordController;
  final bool isBusy;
  final ValueChanged<int> onSwitchTab;
  final Future<void> Function() onLogin;
  final Future<void> Function() onRegister;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: ListView(
          padding: const EdgeInsets.fromLTRB(20, 12, 20, 24),
          children: [
            const _HeroBanner(
              eyebrow: 'Meu Barbeiro',
              title:
                  'Entre como cliente para buscar barbearias e agendar servicos.',
              subtitle: 'Encontre a barbearia ideal e acompanhe seus horarios.',
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
                    selected: {authTab},
                    onSelectionChanged: (selection) =>
                        onSwitchTab(selection.first),
                  ),
                  const SizedBox(height: 20),
                  if (authTab == 0) ...[
                    TextField(
                      controller: loginEmailController,
                      decoration: const InputDecoration(
                        hintText: 'E-mail',
                        prefixIcon: Icon(Icons.mail_outline),
                      ),
                    ),
                    const SizedBox(height: 12),
                    TextField(
                      controller: loginPasswordController,
                      obscureText: true,
                      decoration: const InputDecoration(
                        hintText: 'Senha',
                        prefixIcon: Icon(Icons.lock_outline),
                      ),
                    ),
                    const SizedBox(height: 20),
                    FilledButton(
                      onPressed: isBusy ? null : onLogin,
                      child: isBusy
                          ? const SizedBox(
                              width: 18,
                              height: 18,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            )
                          : const Text('Entrar'),
                    ),
                  ] else ...[
                    TextField(
                      controller: registerNameController,
                      decoration: const InputDecoration(
                        hintText: 'Nome completo',
                        prefixIcon: Icon(Icons.person_outline),
                      ),
                    ),
                    const SizedBox(height: 12),
                    TextField(
                      controller: registerEmailController,
                      decoration: const InputDecoration(
                        hintText: 'E-mail',
                        prefixIcon: Icon(Icons.mail_outline),
                      ),
                    ),
                    const SizedBox(height: 12),
                    TextField(
                      controller: registerPasswordController,
                      obscureText: true,
                      decoration: const InputDecoration(
                        hintText: 'Senha',
                        prefixIcon: Icon(Icons.lock_outline),
                      ),
                    ),
                    const SizedBox(height: 20),
                    FilledButton(
                      onPressed: isBusy ? null : onRegister,
                      child: isBusy
                          ? const SizedBox(
                              width: 18,
                              height: 18,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            )
                          : const Text('Criar conta'),
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
          colors: [Color(0xFF01261F), Color(0xFF1A3C34)],
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
              color: Colors.white.withValues(alpha: 0.12),
              borderRadius: BorderRadius.circular(999),
            ),
            child: Text(
              eyebrow,
              style: theme.textTheme.labelMedium?.copyWith(color: Colors.white),
            ),
          ),
          const SizedBox(height: 18),
          Text(
            title,
            style: theme.textTheme.headlineSmall?.copyWith(
              color: Colors.white,
              fontWeight: FontWeight.w700,
              height: 1.15,
            ),
          ),
          const SizedBox(height: 10),
          Text(
            subtitle,
            style: theme.textTheme.bodyLarge?.copyWith(
              color: Colors.white.withValues(alpha: 0.88),
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

class _ShopCard extends StatelessWidget {
  const _ShopCard({
    required this.shop,
    required this.selected,
    required this.onTap,
  });

  final BarbershopSummary shop;
  final bool selected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return InkWell(
      borderRadius: BorderRadius.circular(24),
      onTap: onTap,
      child: Ink(
        padding: const EdgeInsets.all(20),
        decoration: BoxDecoration(
          color: selected ? const Color(0xFFECF3F0) : Colors.white,
          borderRadius: BorderRadius.circular(24),
          border: Border.all(
            color: selected ? const Color(0xFF1A3C34) : const Color(0xFFE5E7EB),
          ),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Expanded(
                  child: Text(
                    shop.name,
                    style: theme.textTheme.titleMedium?.copyWith(
                      fontWeight: FontWeight.w700,
                    ),
                  ),
                ),
                const Icon(Icons.chevron_right),
              ],
            ),
            const SizedBox(height: 8),
            Text(
              '${shop.city} • ${shop.address}',
              style: theme.textTheme.bodyMedium,
            ),
            const SizedBox(height: 12),
            Text(shop.description, style: theme.textTheme.bodyMedium),
            const SizedBox(height: 12),
            Row(
              children: [
                const Icon(
                  Icons.star_rounded,
                  size: 18,
                  color: Color(0xFF1A3C34),
                ),
                const SizedBox(width: 6),
                Text(
                  shop.averageRating.toStringAsFixed(1),
                  style: theme.textTheme.labelLarge,
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class BarbershopBookingPage extends StatefulWidget {
  const BarbershopBookingPage({
    super.key,
    required this.api,
    required this.baseUrl,
    required this.session,
    required this.shop,
  });

  final BackendApi api;
  final String baseUrl;
  final AuthSession session;
  final BarbershopSummary shop;

  @override
  State<BarbershopBookingPage> createState() => _BarbershopBookingPageState();
}

class _BarbershopBookingPageState extends State<BarbershopBookingPage> {
  final NumberFormat _currency = NumberFormat.currency(
    locale: 'pt_BR',
    symbol: 'R\$',
  );
  bool _isLoadingServices = true;
  bool _isBooking = false;
  String? _servicesError;
  List<ServiceOffering> _services = const [];
  final Set<String> _selectedServiceIds = <String>{};
  DateTime? _selectedDate;
  TimeOfDay? _selectedTime;

  List<DateTime> get _availableDays {
    final now = DateTime.now();
    final start = DateTime(
      now.year,
      now.month,
      now.day,
    ).add(const Duration(days: 1));

    return List<DateTime>.generate(7, (index) {
      final day = start.add(Duration(days: index));
      return DateTime(day.year, day.month, day.day);
    });
  }

  List<TimeOfDay> get _availableTimes {
    return List<TimeOfDay>.generate(
      10,
      (index) => TimeOfDay(hour: 8 + index, minute: 0),
    );
  }

  double get _selectedTotalPrice {
    return _services
        .where((service) => _selectedServiceIds.contains(service.id))
        .fold<double>(0, (sum, service) => sum + service.price);
  }

  int get _selectedDuration {
    return _services
        .where((service) => _selectedServiceIds.contains(service.id))
        .fold<int>(0, (sum, service) => sum + service.durationMinutes);
  }

  @override
  void initState() {
    super.initState();
    _selectedDate = _availableDays.first;
    _selectedTime = _availableTimes.first;
    _loadServices();
  }

  Future<void> _loadServices() async {
    setState(() {
      _isLoadingServices = true;
      _servicesError = null;
      _selectedServiceIds.clear();
    });

    try {
      final services = await widget.api.listServices(
        baseUrl: widget.baseUrl,
        barbershopId: widget.shop.id,
      );

      if (!mounted) {
        return;
      }

      setState(() {
        _services = services;
        _isLoadingServices = false;
      });
    } catch (error) {
      if (!mounted) {
        return;
      }

      setState(() {
        _servicesError = error.toString();
        _isLoadingServices = false;
      });
    }
  }

  Future<void> _bookAppointment() async {
    if (_selectedServiceIds.isEmpty) {
      _showMessage('Selecione pelo menos um servico.');
      return;
    }

    if (_selectedDate == null || _selectedTime == null) {
      _showMessage('Escolha a data e o horario desejados.');
      return;
    }

    final localDateTime = DateTime(
      _selectedDate!.year,
      _selectedDate!.month,
      _selectedDate!.day,
      _selectedTime!.hour,
      _selectedTime!.minute,
    );

    setState(() => _isBooking = true);

    try {
      final appointmentId = await widget.api.createAppointment(
        baseUrl: widget.baseUrl,
        accessToken: widget.session.accessToken,
        barbershopId: widget.shop.id,
        serviceIds: _selectedServiceIds.toList(),
        scheduledAtUtc: localDateTime.toUtc(),
        totalPrice: _selectedTotalPrice,
      );

      if (!mounted) {
        return;
      }

      _showMessage(
        'Agendamento criado com sucesso. Codigo: ${appointmentId.substring(0, 8)}',
      );
      Navigator.of(context).pop(true);
    } catch (error) {
      _showMessage(error.toString());
    } finally {
      if (mounted) {
        setState(() => _isBooking = false);
      }
    }
  }

  void _showMessage(String message) {
    ScaffoldMessenger.of(
      context,
    ).showSnackBar(SnackBar(content: Text(message)));
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(title: const Text('Agendamento')),
      body: ListView(
        padding: const EdgeInsets.fromLTRB(20, 12, 20, 24),
        children: [
          _HeroBanner(
            eyebrow: widget.shop.name,
            title:
                'Escolha servicos, dia e horario para solicitar seu atendimento.',
            subtitle:
                'Os servicos sao carregados da barbearia selecionada. O horario comercial considerado e de 08:00 ate 17:00.',
          ),
          const SizedBox(height: 24),
          _SectionCard(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(widget.shop.name, style: theme.textTheme.titleMedium),
                const SizedBox(height: 4),
                Text(
                  '${widget.shop.city} • ${widget.shop.address}',
                  style: theme.textTheme.bodyMedium,
                ),
                const SizedBox(height: 20),
                Text('Servicos disponiveis', style: theme.textTheme.labelLarge),
                const SizedBox(height: 8),
                if (_isLoadingServices)
                  const Padding(
                    padding: EdgeInsets.symmetric(vertical: 24),
                    child: Center(child: CircularProgressIndicator()),
                  )
                else if (_servicesError != null)
                  Text(_servicesError!, style: theme.textTheme.bodyMedium)
                else if (_services.isEmpty)
                  Text(
                    'Nenhum servico cadastrado para esta barbearia.',
                    style: theme.textTheme.bodyMedium,
                  )
                else
                  for (final service in _services)
                    CheckboxListTile(
                      value: _selectedServiceIds.contains(service.id),
                      contentPadding: EdgeInsets.zero,
                      controlAffinity: ListTileControlAffinity.leading,
                      title: Text(service.name),
                      subtitle: Text(
                        '${service.durationMinutes} min • ${service.description}',
                      ),
                      secondary: Text(_currency.format(service.price)),
                      onChanged: (selected) {
                        setState(() {
                          if (selected == true) {
                            _selectedServiceIds.add(service.id);
                          } else {
                            _selectedServiceIds.remove(service.id);
                          }
                        });
                      },
                    ),
                const SizedBox(height: 12),
                Text('Dias disponiveis', style: theme.textTheme.labelLarge),
                const SizedBox(height: 8),
                Wrap(
                  spacing: 8,
                  runSpacing: 8,
                  children: [
                    for (final day in _availableDays)
                      ChoiceChip(
                        label: Text(
                          DateFormat('EEE dd/MM', 'pt_BR').format(day),
                        ),
                        selected:
                            _selectedDate != null &&
                            _selectedDate!.year == day.year &&
                            _selectedDate!.month == day.month &&
                            _selectedDate!.day == day.day,
                        onSelected: (_) => setState(() => _selectedDate = day),
                      ),
                  ],
                ),
                const SizedBox(height: 16),
                Text('Horarios disponiveis', style: theme.textTheme.labelLarge),
                const SizedBox(height: 8),
                Wrap(
                  spacing: 8,
                  runSpacing: 8,
                  children: [
                    for (final time in _availableTimes)
                      ChoiceChip(
                        label: Text(
                          '${time.hour.toString().padLeft(2, '0')}:${time.minute.toString().padLeft(2, '0')}',
                        ),
                        selected:
                            _selectedTime?.hour == time.hour &&
                            _selectedTime?.minute == time.minute,
                        onSelected: (_) => setState(() => _selectedTime = time),
                      ),
                  ],
                ),
                const SizedBox(height: 20),
                Container(
                  padding: const EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: const Color(0xFFF3F4F5),
                    borderRadius: BorderRadius.circular(18),
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        'Resumo do pedido',
                        style: theme.textTheme.labelLarge,
                      ),
                      const SizedBox(height: 8),
                      Text(
                        'Duracao prevista: $_selectedDuration min',
                        style: theme.textTheme.bodyMedium,
                      ),
                      const SizedBox(height: 4),
                      Text(
                        'Total: ${_currency.format(_selectedTotalPrice)}',
                        style: theme.textTheme.titleMedium,
                      ),
                      const SizedBox(height: 16),
                      FilledButton(
                        onPressed: _isBooking ? null : _bookAppointment,
                        child: _isBooking
                            ? const SizedBox(
                                width: 18,
                                height: 18,
                                child: CircularProgressIndicator(
                                  strokeWidth: 2,
                                ),
                              )
                            : const Text('Solicitar agendamento'),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
