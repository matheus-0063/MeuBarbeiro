import 'package:flutter_test/flutter_test.dart';
import 'package:meu_barbeiro_cliente/main.dart';

void main() {
  testWidgets('renderiza as abas principais do cliente', (tester) async {
    await tester.pumpWidget(const MeuBarbeiroClienteApp());

    expect(find.text('Buscar'), findsOneWidget);
    expect(find.text('Agenda'), findsOneWidget);
    expect(find.text('Perfil'), findsOneWidget);
  });
}
