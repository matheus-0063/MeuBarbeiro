final clientCatalog = [
  BarbershopSeed(
    id: '8f04c6c7-7474-4f39-a473-9b94bd7dc101',
    name: 'Atelie da Barba',
    city: 'Sao Paulo',
    neighborhood: 'Pinheiros',
    rating: 4.9,
    description: 'Acabamento premium, toalha quente e atendimento sem pressa.',
    barbers: [
      BarberSeed(
        id: 'd18a4fd8-a922-4417-a1fc-70472f8a8701',
        name: 'Rafael Costa',
        specialty: 'Fade classico e barba desenhada',
      ),
      BarberSeed(
        id: '1d364dc4-9127-48a4-ae5e-f27f5ea6178e',
        name: 'Lucas Nery',
        specialty: 'Tesoura, social e acabamento executivo',
      ),
    ],
    services: [
      ServiceSeed(
        id: 'svc-corte-premium',
        name: 'Corte premium',
        durationMinutes: 50,
        price: 65,
      ),
      ServiceSeed(
        id: 'svc-barba-classica',
        name: 'Barba classica',
        durationMinutes: 35,
        price: 40,
      ),
      ServiceSeed(
        id: 'svc-combo-executivo',
        name: 'Combo executivo',
        durationMinutes: 80,
        price: 95,
      ),
    ],
    availableSlots: [
      '2026-06-09T13:00:00',
      '2026-06-09T15:30:00',
      '2026-06-10T10:00:00',
      '2026-06-10T14:30:00',
    ],
  ),
  BarbershopSeed(
    id: '8d31b8ad-5a51-4d44-8f8b-3f7bd7bfa202',
    name: 'Studio Navalha',
    city: 'Sao Paulo',
    neighborhood: 'Vila Mariana',
    rating: 4.8,
    description: 'Ambiente minimalista e servico rapido para rotina corrida.',
    barbers: [
      BarberSeed(
        id: 'f6f8f4d5-5d9d-4a67-b754-5da2d2cf7202',
        name: 'Vinicius Prado',
        specialty: 'Corte degradado e pigmentacao',
      ),
      BarberSeed(
        id: '74a4d0e7-c5e8-4a06-8969-5eb9e70fb7f1',
        name: 'Murilo Teixeira',
        specialty: 'Barba tradicional e acabamento navalhado',
      ),
    ],
    services: [
      ServiceSeed(
        id: 'svc-corte-rapido',
        name: 'Corte rapido',
        durationMinutes: 35,
        price: 45,
      ),
      ServiceSeed(
        id: 'svc-barba-premium',
        name: 'Barba premium',
        durationMinutes: 40,
        price: 45,
      ),
      ServiceSeed(
        id: 'svc-combo-studio',
        name: 'Combo studio',
        durationMinutes: 70,
        price: 82,
      ),
    ],
    availableSlots: [
      '2026-06-09T11:00:00',
      '2026-06-09T16:00:00',
      '2026-06-11T09:30:00',
      '2026-06-11T13:30:00',
    ],
  ),
  BarbershopSeed(
    id: 'bf083e72-53be-4d68-a257-35187b8db303',
    name: 'Casa Norte Barber Club',
    city: 'Guarulhos',
    neighborhood: 'Centro',
    rating: 4.7,
    description: 'Foco em fidelizacao, combos e agenda noturna.',
    barbers: [
      BarberSeed(
        id: '6d99e28f-68a7-4b42-b8d8-f35e0a31b0a5',
        name: 'Andre Mota',
        specialty: 'Cortes modernos e alinhamento de barba',
      ),
    ],
    services: [
      ServiceSeed(
        id: 'svc-corte-club',
        name: 'Corte club',
        durationMinutes: 45,
        price: 50,
      ),
      ServiceSeed(
        id: 'svc-combo-noite',
        name: 'Combo noite',
        durationMinutes: 75,
        price: 88,
      ),
    ],
    availableSlots: [
      '2026-06-10T18:00:00',
      '2026-06-10T19:30:00',
      '2026-06-12T17:00:00',
    ],
  ),
];

class BarbershopSeed {
  const BarbershopSeed({
    required this.id,
    required this.name,
    required this.city,
    required this.neighborhood,
    required this.rating,
    required this.description,
    required this.barbers,
    required this.services,
    required this.availableSlots,
  });

  final String id;
  final String name;
  final String city;
  final String neighborhood;
  final double rating;
  final String description;
  final List<BarberSeed> barbers;
  final List<ServiceSeed> services;
  final List<String> availableSlots;
}

class BarberSeed {
  const BarberSeed({
    required this.id,
    required this.name,
    required this.specialty,
  });

  final String id;
  final String name;
  final String specialty;
}

class ServiceSeed {
  const ServiceSeed({
    required this.id,
    required this.name,
    required this.durationMinutes,
    required this.price,
  });

  final String id;
  final String name;
  final int durationMinutes;
  final double price;
}
