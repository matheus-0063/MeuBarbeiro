import 'package:flutter_test/flutter_test.dart';
import 'package:meu_barbeiro_cliente/main.dart';
import 'package:shared_preferences/shared_preferences.dart';

void main() {
  testWidgets('renderiza tela de autenticacao do cliente', (tester) async {
    SharedPreferences.setMockInitialValues({});

    await tester.pumpWidget(const MeuBarbeiroClienteApp());
    await tester.pumpAndSettle();

    expect(find.text('Login'), findsOneWidget);
    expect(find.text('Cadastro'), findsOneWidget);
    expect(find.text('Entrar'), findsOneWidget);
  });
}
