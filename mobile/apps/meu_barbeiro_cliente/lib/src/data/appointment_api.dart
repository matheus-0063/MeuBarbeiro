import 'dart:convert';

import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;

class AppointmentApiClient {
  AppointmentApiClient({http.Client? httpClient})
    : _httpClient = httpClient ?? http.Client();

  final http.Client _httpClient;

  Future<List<ClientAppointment>> listAppointments({
    required String baseUrl,
    required String clientId,
    AppointmentStatusFilter? status,
  }) async {
    final uri = Uri.parse('$baseUrl/api/v1/appointment').replace(
      queryParameters: {
        'userId': clientId,
        'userType': '1',
        if (status != null) 'status': status.apiValue,
      },
    );

    final response = await _httpClient.get(uri);
    if (response.statusCode != 200) {
      throw AppointmentApiException(_readMessage(response));
    }

    final decoded = jsonDecode(response.body) as List<dynamic>;
    return decoded
        .map((item) => ClientAppointment.fromJson(item as Map<String, dynamic>))
        .toList();
  }

  Future<String> createAppointment({
    required String baseUrl,
    required AppointmentDraft draft,
  }) async {
    final response = await _httpClient.post(
      Uri.parse('$baseUrl/api/v1/appointment'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'clientId': draft.clientId,
        'barberId': draft.barberId,
        'barbershopId': draft.barbershopId,
        'scheduledAtUtc': draft.scheduledAtUtc.toIso8601String(),
        'totalPrice': draft.totalPrice,
      }),
    );

    if (response.statusCode != 201) {
      throw AppointmentApiException(_readMessage(response));
    }

    final decoded = jsonDecode(response.body) as Map<String, dynamic>;
    return decoded['appointmentId'] as String;
  }

  String _readMessage(http.Response response) {
    try {
      final decoded = jsonDecode(response.body);
      if (decoded is Map<String, dynamic>) {
        final title = decoded['title'];
        if (title is String && title.isNotEmpty) {
          return title;
        }
      }
    } catch (_) {
      // Algumas respostas da API podem nao vir como JSON estruturado.
    }

    return 'Falha ao comunicar com a API (${response.statusCode}).';
  }
}

@immutable
class AppointmentDraft {
  const AppointmentDraft({
    required this.clientId,
    required this.barberId,
    required this.barbershopId,
    required this.scheduledAtUtc,
    required this.totalPrice,
  });

  final String clientId;
  final String barberId;
  final String barbershopId;
  final DateTime scheduledAtUtc;
  final double totalPrice;
}

@immutable
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
  completed('Completed', 'Concluidos');

  const AppointmentStatusFilter(this.apiValue, this.label);

  final String apiValue;
  final String label;
}

class AppointmentApiException implements Exception {
  AppointmentApiException(this.message);

  final String message;

  @override
  String toString() => message;
}
