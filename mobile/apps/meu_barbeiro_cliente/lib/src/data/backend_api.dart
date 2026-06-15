import 'dart:convert';

import 'package:http/http.dart' as http;

class BackendApi {
  BackendApi({http.Client? httpClient})
    : _httpClient = httpClient ?? http.Client();

  final http.Client _httpClient;

  Future<AuthSession> login({
    required String baseUrl,
    required String email,
    required String password,
  }) async {
    final response = await _httpClient.post(
      Uri.parse('$baseUrl/api/v1/auth/login'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'email': email, 'password': password}),
    );

    return _parseAuthResponse(response);
  }

  Future<AuthSession> registerClient({
    required String baseUrl,
    required String name,
    required String email,
    required String password,
  }) async {
    final response = await _httpClient.post(
      Uri.parse('$baseUrl/api/v1/auth/register/client'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({'name': name, 'email': email, 'password': password}),
    );

    return _parseAuthResponse(response);
  }

  Future<List<BarbershopSummary>> listBarbershops({
    required String baseUrl,
    String? city,
  }) async {
    final uri = Uri.parse('$baseUrl/api/v1/barbershop').replace(
      queryParameters: {
        if (city != null && city.trim().isNotEmpty) 'city': city.trim(),
      },
    );

    final response = await _httpClient.get(uri);
    if (response.statusCode != 200) {
      throw BackendApiException(_extractErrorMessage(response));
    }

    final decoded = jsonDecode(response.body) as List<dynamic>;
    return decoded
        .map((item) => BarbershopSummary.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<BarbershopSummary> getBarbershop({
    required String baseUrl,
    required String barbershopId,
  }) async {
    final response = await _httpClient.get(
      Uri.parse('$baseUrl/api/v1/barbershop/$barbershopId'),
    );

    if (response.statusCode != 200) {
      throw BackendApiException(_extractErrorMessage(response));
    }

    return BarbershopSummary.fromJson(
      jsonDecode(response.body) as Map<String, dynamic>,
    );
  }

  Future<List<ServiceOffering>> listServices({
    required String baseUrl,
    required String barbershopId,
  }) async {
    final uri = Uri.parse(
      '$baseUrl/api/v1/services',
    ).replace(queryParameters: {'barbershopId': barbershopId});

    final response = await _httpClient.get(uri);
    if (response.statusCode != 200) {
      throw BackendApiException(_extractErrorMessage(response));
    }

    final decoded = jsonDecode(response.body) as List<dynamic>;
    return decoded
        .map((item) => ServiceOffering.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<List<ClientAppointment>> getMyAppointments({
    required String baseUrl,
    required String accessToken,
    AppointmentStatusFilter? status,
  }) async {
    final uri = Uri.parse(
      '$baseUrl/api/v1/appointment/mine',
    ).replace(queryParameters: {if (status != null) 'status': status.apiValue});

    final response = await _httpClient.get(
      uri,
      headers: _authHeaders(accessToken),
    );

    if (response.statusCode != 200) {
      throw BackendApiException(_extractErrorMessage(response));
    }

    final decoded = jsonDecode(response.body) as List<dynamic>;
    return decoded
        .map((item) => ClientAppointment.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<String> createAppointment({
    required String baseUrl,
    required String accessToken,
    required String barbershopId,
    required DateTime scheduledAtUtc,
    required double totalPrice,
  }) async {
    final response = await _httpClient.post(
      Uri.parse('$baseUrl/api/v1/appointment'),
      headers: _authHeaders(accessToken),
      body: jsonEncode({
        'barbershopId': barbershopId,
        'scheduledAtUtc': scheduledAtUtc.toIso8601String(),
        'totalPrice': totalPrice,
      }),
    );

    if (response.statusCode != 201) {
      throw BackendApiException(_extractErrorMessage(response));
    }

    final decoded = jsonDecode(response.body) as Map<String, dynamic>;
    return decoded['appointmentId'] as String;
  }

  AuthSession _parseAuthResponse(http.Response response) {
    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw BackendApiException(_extractErrorMessage(response));
    }

    return AuthSession.fromJson(
      jsonDecode(response.body) as Map<String, dynamic>,
    );
  }

  Map<String, String> _authHeaders(String accessToken) {
    return {
      'Content-Type': 'application/json',
      'Authorization': 'Bearer $accessToken',
    };
  }

  String _extractErrorMessage(http.Response response) {
    final fallback = 'Falha ao comunicar com a API (${response.statusCode}).';

    try {
      final decoded = jsonDecode(response.body);

      if (decoded is Map<String, dynamic>) {
        final errors = decoded['errors'];
        if (errors is Map<String, dynamic>) {
          final flattened = <String>[];

          for (final value in errors.values) {
            if (value is List) {
              flattened.addAll(value.map((item) => item.toString()));
            } else if (value != null) {
              flattened.add(value.toString());
            }
          }

          if (flattened.isNotEmpty) {
            return flattened.join('\n');
          }
        }

        final title = decoded['title'];
        if (title is String && title.isNotEmpty) {
          return title;
        }

        final errorMessages = decoded['ErrorMessages'];
        if (errorMessages is List && errorMessages.isNotEmpty) {
          return errorMessages.join('\n');
        }
      }
    } catch (_) {
      final body = response.body.trim();
      if (body.isNotEmpty) {
        return body;
      }
    }

    return fallback;
  }
}

class AuthSession {
  const AuthSession({
    required this.accessToken,
    required this.userId,
    required this.name,
    required this.email,
    required this.role,
  });

  factory AuthSession.fromJson(Map<String, dynamic> json) {
    return AuthSession(
      accessToken: json['accessToken'] as String,
      userId: json['userId'] as String,
      name: json['name'] as String,
      email: json['email'] as String,
      role: json['role'] as String,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'accessToken': accessToken,
      'userId': userId,
      'name': name,
      'email': email,
      'role': role,
    };
  }

  final String accessToken;
  final String userId;
  final String name;
  final String email;
  final String role;
}

class BarbershopSummary {
  const BarbershopSummary({
    required this.id,
    required this.name,
    required this.city,
    required this.address,
    required this.description,
    required this.averageRating,
  });

  factory BarbershopSummary.fromJson(Map<String, dynamic> json) {
    return BarbershopSummary(
      id: json['id'] as String,
      name: json['name'] as String,
      city: json['city'] as String,
      address: json['address'] as String,
      description: json['description'] as String,
      averageRating: (json['averageRating'] as num).toDouble(),
    );
  }

  final String id;
  final String name;
  final String city;
  final String address;
  final String description;
  final double averageRating;
}

class ServiceOffering {
  const ServiceOffering({
    required this.id,
    required this.barbershopId,
    required this.name,
    required this.price,
    required this.description,
    required this.durationMinutes,
  });

  factory ServiceOffering.fromJson(Map<String, dynamic> json) {
    return ServiceOffering(
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

class ClientAppointment {
  const ClientAppointment({
    required this.id,
    required this.clientId,
    required this.barberId,
    required this.barbershopId,
    required this.scheduledAtUtc,
    required this.totalPrice,
    required this.status,
  });

  factory ClientAppointment.fromJson(Map<String, dynamic> json) {
    return ClientAppointment(
      id: json['id'] as String,
      clientId: json['clientId'] as String,
      barberId: json['barberId'] as String,
      barbershopId: json['barbershopId'] as String,
      scheduledAtUtc: DateTime.parse(json['scheduledAtUtc'] as String),
      totalPrice: (json['totalPrice'] as num).toDouble(),
      status: json['status'] as String,
    );
  }

  final String id;
  final String clientId;
  final String barberId;
  final String barbershopId;
  final DateTime scheduledAtUtc;
  final double totalPrice;
  final String status;
}

enum AppointmentStatusFilter {
  pending('Pending', 'Pendentes'),
  accepted('Accepted', 'Aceitos'),
  rejected('Rejected', 'Recusados'),
  inProgress('InProgress', 'Em andamento'),
  completed('Completed', 'Concluidos'),
  cancelled('Cancelled', 'Cancelados');

  const AppointmentStatusFilter(this.apiValue, this.label);

  final String apiValue;
  final String label;
}

class BackendApiException implements Exception {
  BackendApiException(this.message);

  final String message;

  @override
  String toString() => message;
}
