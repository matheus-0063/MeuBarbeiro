import 'package:flutter_test/flutter_test.dart';
import 'package:meu_barbeiro_cliente/main.dart';

void main() {
  testWidgets('renderiza o dashboard inicial do cliente', (tester) async {
    await tester.pumpWidget(const MeuBarbeiroClienteApp());

    expect(find.text('MeuBarbeiro Cliente'), findsOneWidget);
    expect(find.text('Buscar por cidade'), findsOneWidget);
    expect(find.text('Avaliar servico'), findsOneWidget);
  });
}
