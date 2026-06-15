import 'package:flutter_test/flutter_test.dart';
import 'package:meu_barbeiro_prestador/main.dart';

void main() {
  testWidgets('renderiza autenticacao do prestador', (tester) async {
    await tester.pumpWidget(const MeuBarbeiroPrestadorApp());

    expect(find.text('Login'), findsOneWidget);
    expect(find.text('Cadastro'), findsOneWidget);
    expect(find.text('Criar conta de barbeiro'), findsNothing);
  });
}
