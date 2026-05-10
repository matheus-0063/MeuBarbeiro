import 'package:flutter_test/flutter_test.dart';
import 'package:meu_barbeiro_prestador/main.dart';

void main() {
  testWidgets('renderiza o dashboard inicial do prestador', (tester) async {
    await tester.pumpWidget(const MeuBarbeiroPrestadorApp());

    expect(find.text('MeuBarbeiro Prestador'), findsOneWidget);
    expect(find.text('Novas solicitacoes'), findsOneWidget);
    expect(find.text('Aceitar ou recusar'), findsOneWidget);
  });
}
