import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../data/appointment_api.dart';
import '../data/client_catalog.dart';

class ClientShellPage extends StatefulWidget {
  const ClientShellPage({super.key});

  @override
  State<ClientShellPage> createState() => _ClientShellPageState();
}

class _ClientShellPageState extends State<ClientShellPage> {
  static const _defaultClientId = '0f8fad5b-d9cb-469f-a165-70867728950e';
  static const _defaultApiBaseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'http://localhost:5039',
  );

  final AppointmentApiClient _apiClient = AppointmentApiClient();
  final NumberFormat _currency = NumberFormat.currency(
    locale: 'pt_BR',
    symbol: 'R\$',
  );
  final TextEditingController _cityController = TextEditingController(
    text: 'Sao Paulo',
  );
  final TextEditingController _clientIdController = TextEditingController(
    text: _defaultClientId,
  );
  final TextEditingController _apiUrlController = TextEditingController(
    text: _defaultApiBaseUrl,
  );

  int _currentIndex = 0;
  int _selectedShopIndex = 0;
  int _selectedBarberIndex = 0;
  int _selectedSlotIndex = 0;
  bool _isSubmitting = false;
  bool _isLoadingAppointments = true;
  String? _appointmentsError;
  AppointmentStatusFilter? _statusFilter;
  List<ClientAppointment> _appointments = const [];
  final Set<String> _selectedServiceIds = <String>{};

  @override
  void initState() {
    super.initState();
    _selectedServiceIds.addAll(
      clientCatalog.first.services.take(2).map((service) => service.id),
    );
    _loadAppointments();
  }

  @override
  void dispose() {
    _cityController.dispose();
    _clientIdController.dispose();
    _apiUrlController.dispose();
    super.dispose();
  }

  List<BarbershopSeed> get _filteredShops {
    final city = _cityController.text.trim().toLowerCase();
    if (city.isEmpty) {
      return clientCatalog;
    }

    final filtered = clientCatalog
        .where((shop) => shop.city.toLowerCase().contains(city))
        .toList();
    return filtered.isEmpty ? clientCatalog : filtered;
  }

  BarbershopSeed get _selectedShop {
    final index = _safeIndex(_selectedShopIndex, _filteredShops.length);
    return _filteredShops[index];
  }

  BarberSeed get _selectedBarber {
    final barbers = _selectedShop.barbers;
    final index = _safeIndex(_selectedBarberIndex, barbers.length);
    return barbers[index];
  }

  List<ServiceSeed> get _selectedServices => _selectedShop.services
      .where((service) => _selectedServiceIds.contains(service.id))
      .toList();

  double get _totalPrice =>
      _selectedServices.fold<double>(0, (sum, service) => sum + service.price);

  DateTime get _selectedSlot {
    final slots = _selectedShop.availableSlots;
    final index = _safeIndex(_selectedSlotIndex, slots.length);
    return DateTime.parse(slots[index]).toUtc();
  }

  int _safeIndex(int index, int length) {
    if (length == 0) {
      return 0;
    }

    if (index < 0) {
      return 0;
    }

    if (index >= length) {
      return length - 1;
    }

    return index;
  }

  Future<void> _loadAppointments() async {
    setState(() {
      _isLoadingAppointments = true;
      _appointmentsError = null;
    });

    try {
      final appointments = await _apiClient.listAppointments(
        baseUrl: _apiUrlController.text.trim(),
        clientId: _clientIdController.text.trim(),
        status: _statusFilter,
      );

      if (!mounted) {
        return;
      }

      setState(() {
        _appointments = appointments;
        _isLoadingAppointments = false;
      });
    } catch (error) {
      if (!mounted) {
        return;
      }

      setState(() {
        _appointmentsError = error.toString();
        _isLoadingAppointments = false;
      });
    }
  }

  Future<void> _submitAppointment() async {
    if (_selectedServices.isEmpty) {
      _showMessage('Selecione ao menos um servico para continuar.');
      return;
    }

    setState(() => _isSubmitting = true);

    try {
      final appointmentId = await _apiClient.createAppointment(
        baseUrl: _apiUrlController.text.trim(),
        draft: AppointmentDraft(
          clientId: _clientIdController.text.trim(),
          barberId: _selectedBarber.id,
          barbershopId: _selectedShop.id,
          scheduledAtUtc: _selectedSlot,
          totalPrice: _totalPrice,
        ),
      );

      if (!mounted) {
        return;
      }

      _showMessage(
        'Agendamento criado com sucesso. Codigo: ${appointmentId.substring(0, 8)}',
      );
      setState(() => _currentIndex = 1);
      await _loadAppointments();
    } catch (error) {
      _showMessage(error.toString());
    } finally {
      if (mounted) {
        setState(() => _isSubmitting = false);
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
    return Scaffold(
      body: SafeArea(
        child: IndexedStack(
          index: _currentIndex,
          children: [
            _buildDiscoverPage(context),
            _buildAppointmentsPage(context),
            _buildProfilePage(context),
          ],
        ),
      ),
      bottomNavigationBar: NavigationBar(
        selectedIndex: _currentIndex,
        onDestinationSelected: (index) => setState(() => _currentIndex = index),
        destinations: const [
          NavigationDestination(
            icon: Icon(Icons.storefront_outlined),
            label: 'Buscar',
          ),
          NavigationDestination(
            icon: Icon(Icons.event_note_outlined),
            label: 'Agenda',
          ),
          NavigationDestination(
            icon: Icon(Icons.person_outline),
            label: 'Perfil',
          ),
        ],
      ),
    );
  }

  Widget _buildDiscoverPage(BuildContext context) {
    final theme = Theme.of(context);
    final shop = _selectedShop;

    return CustomScrollView(
      slivers: [
        SliverToBoxAdapter(
          child: Padding(
            padding: const EdgeInsets.fromLTRB(20, 12, 20, 24),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const _HeroBanner(
                  eyebrow: 'Cliente',
                  title: 'Agende com calma e acompanhe o status em tempo real.',
                  subtitle:
                      'O catalogo segue o design do Stitch e o envio usa o endpoint real de agendamento.',
                ),
                const SizedBox(height: 24),
                Text('Cidade', style: theme.textTheme.labelLarge),
                const SizedBox(height: 8),
                TextField(
                  controller: _cityController,
                  decoration: const InputDecoration(
                    hintText: 'Ex.: Sao Paulo',
                    prefixIcon: Icon(Icons.location_on_outlined),
                  ),
                  onChanged: (_) {
                    setState(() {
                      _selectedShopIndex = 0;
                      _selectedBarberIndex = 0;
                      _selectedSlotIndex = 0;
                    });
                  },
                ),
                const SizedBox(height: 24),
                Text(
                  'Barbearias disponiveis',
                  style: theme.textTheme.titleLarge,
                ),
                const SizedBox(height: 12),
                for (final entry in _filteredShops.asMap().entries)
                  Padding(
                    padding: const EdgeInsets.only(bottom: 12),
                    child: _ShopCard(
                      shop: entry.value,
                      selected: _selectedShopIndex == entry.key,
                      onTap: () {
                        setState(() {
                          _selectedShopIndex = entry.key;
                          _selectedBarberIndex = 0;
                          _selectedSlotIndex = 0;
                          _selectedServiceIds
                            ..clear()
                            ..add(entry.value.services.first.id);
                        });
                      },
                    ),
                  ),
                const SizedBox(height: 24),
                Text('Agendamento', style: theme.textTheme.titleLarge),
                const SizedBox(height: 12),
                _SectionCard(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(shop.name, style: theme.textTheme.titleMedium),
                      const SizedBox(height: 4),
                      Text(
                        '${shop.neighborhood} • ${shop.city}',
                        style: theme.textTheme.bodyMedium,
                      ),
                      const SizedBox(height: 16),
                      Text('Barbeiro', style: theme.textTheme.labelLarge),
                      const SizedBox(height: 8),
                      Wrap(
                        spacing: 8,
                        runSpacing: 8,
                        children: [
                          for (final entry in shop.barbers.asMap().entries)
                            ChoiceChip(
                              label: Text(entry.value.name),
                              selected: _selectedBarberIndex == entry.key,
                              onSelected: (_) => setState(
                                () => _selectedBarberIndex = entry.key,
                              ),
                            ),
                        ],
                      ),
                      const SizedBox(height: 16),
                      Text(
                        _selectedBarber.specialty,
                        style: theme.textTheme.bodySmall,
                      ),
                      const SizedBox(height: 20),
                      Text('Servicos', style: theme.textTheme.labelLarge),
                      const SizedBox(height: 8),
                      for (final service in shop.services)
                        CheckboxListTile(
                          value: _selectedServiceIds.contains(service.id),
                          contentPadding: EdgeInsets.zero,
                          controlAffinity: ListTileControlAffinity.leading,
                          title: Text(service.name),
                          subtitle: Text('${service.durationMinutes} min'),
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
                      Text('Horario', style: theme.textTheme.labelLarge),
                      const SizedBox(height: 8),
                      Wrap(
                        spacing: 8,
                        runSpacing: 8,
                        children: [
                          for (final entry
                              in shop.availableSlots.asMap().entries)
                            ChoiceChip(
                              label: Text(_slotLabel(entry.value)),
                              selected: _selectedSlotIndex == entry.key,
                              onSelected: (_) => setState(
                                () => _selectedSlotIndex = entry.key,
                              ),
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
                        child: Row(
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: [
                            Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(
                                  'Total',
                                  style: theme.textTheme.labelLarge,
                                ),
                                const SizedBox(height: 4),
                                Text(
                                  _currency.format(_totalPrice),
                                  style: theme.textTheme.titleLarge?.copyWith(
                                    fontWeight: FontWeight.w700,
                                  ),
                                ),
                              ],
                            ),
                            FilledButton(
                              onPressed: _isSubmitting
                                  ? null
                                  : _submitAppointment,
                              child: _isSubmitting
                                  ? const SizedBox(
                                      width: 18,
                                      height: 18,
                                      child: CircularProgressIndicator(
                                        strokeWidth: 2,
                                      ),
                                    )
                                  : const Text('Confirmar'),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildAppointmentsPage(BuildContext context) {
    final theme = Theme.of(context);

    return RefreshIndicator(
      onRefresh: _loadAppointments,
      child: ListView(
        padding: const EdgeInsets.fromLTRB(20, 12, 20, 24),
        children: [
          const _HeroBanner(
            eyebrow: 'Agenda',
            title: 'Seus agendamentos carregados direto da API.',
            subtitle:
                'A listagem usa GET de appointments com filtro por cliente e atualizacao manual por pull-to-refresh.',
          ),
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
                    onSelected: (_) {
                      setState(() => _statusFilter = null);
                      _loadAppointments();
                    },
                  ),
                ),
                for (final filter in AppointmentStatusFilter.values)
                  Padding(
                    padding: const EdgeInsets.only(right: 8),
                    child: ChoiceChip(
                      label: Text(filter.label),
                      selected: _statusFilter == filter,
                      onSelected: (_) {
                        setState(() => _statusFilter = filter);
                        _loadAppointments();
                      },
                    ),
                  ),
              ],
            ),
          ),
          const SizedBox(height: 20),
          if (_isLoadingAppointments)
            const Padding(
              padding: EdgeInsets.only(top: 40),
              child: Center(child: CircularProgressIndicator()),
            )
          else if (_appointmentsError != null)
            _SectionCard(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Nao foi possivel carregar.',
                    style: theme.textTheme.titleMedium,
                  ),
                  const SizedBox(height: 8),
                  Text(_appointmentsError!, style: theme.textTheme.bodyMedium),
                  const SizedBox(height: 16),
                  OutlinedButton(
                    onPressed: _loadAppointments,
                    child: const Text('Tentar novamente'),
                  ),
                ],
              ),
            )
          else if (_appointments.isEmpty)
            _SectionCard(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Nenhum agendamento encontrado.',
                    style: theme.textTheme.titleMedium,
                  ),
                  const SizedBox(height: 8),
                  Text(
                    'Crie seu primeiro agendamento na aba Buscar ou ajuste o Client ID no perfil.',
                    style: theme.textTheme.bodyMedium,
                  ),
                ],
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
                              _shopNameById(appointment.barbershopId),
                              style: theme.textTheme.titleMedium,
                            ),
                          ),
                          _StatusPill(status: appointment.status),
                        ],
                      ),
                      const SizedBox(height: 10),
                      Text(
                        _barberNameById(appointment.barberId),
                        style: theme.textTheme.bodyLarge,
                      ),
                      const SizedBox(height: 6),
                      Text(
                        _dateTimeLabel(appointment.scheduledAtUtc),
                        style: theme.textTheme.bodyMedium,
                      ),
                      const SizedBox(height: 6),
                      Text(
                        _currency.format(appointment.totalPrice),
                        style: theme.textTheme.labelLarge,
                      ),
                      const SizedBox(height: 14),
                      Text(
                        'ID: ${appointment.id}',
                        style: theme.textTheme.bodySmall?.copyWith(
                          color: const Color(0xFF5A5F64),
                        ),
                      ),
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
        const _HeroBanner(
          eyebrow: 'Configuracao',
          title: 'Ajuste a conexao com o backend sem recompilar o app.',
          subtitle:
              'Isso ajuda ao alternar entre simulador, emulador Android e aparelho fisico.',
        ),
        const SizedBox(height: 24),
        _SectionCard(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text('Client ID', style: theme.textTheme.labelLarge),
              const SizedBox(height: 8),
              TextField(
                controller: _clientIdController,
                decoration: const InputDecoration(
                  hintText: 'GUID do cliente',
                  prefixIcon: Icon(Icons.badge_outlined),
                ),
              ),
              const SizedBox(height: 20),
              Text('API base URL', style: theme.textTheme.labelLarge),
              const SizedBox(height: 8),
              TextField(
                controller: _apiUrlController,
                decoration: const InputDecoration(
                  hintText: 'http://localhost:5039',
                  prefixIcon: Icon(Icons.link_outlined),
                ),
              ),
              const SizedBox(height: 12),
              Text(
                'No Android emulator, normalmente voce vai usar http://10.0.2.2:5039.',
                style: theme.textTheme.bodySmall,
              ),
              const SizedBox(height: 20),
              FilledButton.icon(
                onPressed: _loadAppointments,
                icon: const Icon(Icons.sync),
                label: const Text('Salvar e recarregar agenda'),
              ),
            ],
          ),
        ),
        const SizedBox(height: 16),
        _SectionCard(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'Cobertura atual do backend',
                style: theme.textTheme.titleMedium,
              ),
              const SizedBox(height: 12),
              const _InfoRow(
                icon: Icons.check_circle_outline,
                title: 'Criacao de agendamento',
                subtitle: 'POST /api/v1/appointment',
              ),
              const _InfoRow(
                icon: Icons.check_circle_outline,
                title: 'Listagem por cliente',
                subtitle: 'GET /api/v1/appointment?userId=...&userType=1',
              ),
              const _InfoRow(
                icon: Icons.pending_outlined,
                title: 'Busca de barbearias',
                subtitle:
                    'Ainda nao existe endpoint; app usa catalogo local temporario.',
              ),
              const _InfoRow(
                icon: Icons.pending_outlined,
                title: 'Login/cadastro e avaliacoes',
                subtitle: 'Estrutura pronta na UI, aguardando endpoints.',
              ),
            ],
          ),
        ),
      ],
    );
  }

  String _slotLabel(String isoString) {
    final date = DateTime.parse(isoString);
    return DateFormat('dd/MM • HH:mm').format(date);
  }

  String _dateTimeLabel(DateTime dateTime) {
    return DateFormat(
      "dd 'de' MMMM • HH:mm",
      'pt_BR',
    ).format(dateTime.toLocal());
  }

  String _shopNameById(String id) {
    for (final shop in clientCatalog) {
      if (shop.id == id) {
        return shop.name;
      }
    }

    return 'Barbearia';
  }

  String _barberNameById(String id) {
    for (final shop in clientCatalog) {
      for (final barber in shop.barbers) {
        if (barber.id == id) {
          return barber.name;
        }
      }
    }

    return 'Barbeiro';
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

  final BarbershopSeed shop;
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
              '${shop.neighborhood} • ${shop.city}',
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
                Text('${shop.rating}', style: theme.textTheme.labelLarge),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class _StatusPill extends StatelessWidget {
  const _StatusPill({required this.status});

  final String status;

  @override
  Widget build(BuildContext context) {
    Color background;
    Color foreground;
    String label;

    switch (status) {
      case 'Accepted':
        background = const Color(0xFFDFF4E8);
        foreground = const Color(0xFF1E6B43);
        label = 'Aceito';
        break;
      case 'Rejected':
        background = const Color(0xFFFFE2DE);
        foreground = const Color(0xFF8B2E23);
        label = 'Recusado';
        break;
      case 'Completed':
        background = const Color(0xFFE3ECFF);
        foreground = const Color(0xFF274F9F);
        label = 'Concluido';
        break;
      default:
        background = const Color(0xFFF3F4F5);
        foreground = const Color(0xFF414846);
        label = 'Pendente';
    }

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

class _InfoRow extends StatelessWidget {
  const _InfoRow({
    required this.icon,
    required this.title,
    required this.subtitle,
  });

  final IconData icon;
  final String title;
  final String subtitle;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Padding(
            padding: const EdgeInsets.only(top: 2),
            child: Icon(icon, size: 18, color: const Color(0xFF1A3C34)),
          ),
          const SizedBox(width: 10),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(title, style: theme.textTheme.labelLarge),
                const SizedBox(height: 2),
                Text(subtitle, style: theme.textTheme.bodySmall),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
