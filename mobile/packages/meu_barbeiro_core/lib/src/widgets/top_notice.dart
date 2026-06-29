import 'dart:async';

import 'package:flutter/material.dart';

enum TopNoticeType { success, error, info }

void showTopNotice(
  BuildContext context, {
  required String message,
  TopNoticeType type = TopNoticeType.info,
  Duration duration = const Duration(seconds: 3),
}) {
  final overlay = Overlay.of(context, rootOverlay: true);
  late final OverlayEntry entry;
  Timer? dismissTimer;

  void removeEntry() {
    dismissTimer?.cancel();
    entry.remove();
  }

  entry = OverlayEntry(
    builder: (context) => _TopNoticeOverlay(
      message: message,
      type: type,
      duration: duration,
      onDismissed: removeEntry,
    ),
  );

  overlay.insert(entry);
  dismissTimer = Timer(
    duration + const Duration(milliseconds: 450),
    removeEntry,
  );
}

class _TopNoticeOverlay extends StatefulWidget {
  const _TopNoticeOverlay({
    required this.message,
    required this.type,
    required this.duration,
    required this.onDismissed,
  });

  final String message;
  final TopNoticeType type;
  final Duration duration;
  final VoidCallback onDismissed;

  @override
  State<_TopNoticeOverlay> createState() => _TopNoticeOverlayState();
}

class _TopNoticeOverlayState extends State<_TopNoticeOverlay> {
  bool _visible = false;
  bool _dismissed = false;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (mounted) {
        setState(() => _visible = true);
      }
    });

    Future<void>.delayed(widget.duration, () {
      if (mounted) {
        _dismiss();
      }
    });
  }

  void _dismiss() {
    if (_dismissed) {
      return;
    }

    _dismissed = true;
    setState(() => _visible = false);
    Future<void>.delayed(const Duration(milliseconds: 260), widget.onDismissed);
  }

  @override
  Widget build(BuildContext context) {
    final palette = switch (widget.type) {
      TopNoticeType.success => (
        gradient: const [Color(0xFF0F5B47), Color(0xFF1B8A6B)],
        icon: Icons.check_circle_rounded,
      ),
      TopNoticeType.error => (
        gradient: const [Color(0xFF7A1F2B), Color(0xFFC2485C)],
        icon: Icons.error_rounded,
      ),
      TopNoticeType.info => (
        gradient: const [Color(0xFF183B55), Color(0xFF2C7AA3)],
        icon: Icons.notifications_active_rounded,
      ),
    };

    return IgnorePointer(
      ignoring: false,
      child: SafeArea(
        child: Align(
          alignment: Alignment.topCenter,
          child: Padding(
            padding: const EdgeInsets.fromLTRB(16, 12, 16, 0),
            child: AnimatedSlide(
              offset: _visible ? Offset.zero : const Offset(0, -0.25),
              duration: const Duration(milliseconds: 260),
              curve: Curves.easeOutCubic,
              child: AnimatedOpacity(
                opacity: _visible ? 1 : 0,
                duration: const Duration(milliseconds: 220),
                child: Material(
                  color: Colors.transparent,
                  child: Container(
                    constraints: const BoxConstraints(maxWidth: 560),
                    decoration: BoxDecoration(
                      gradient: LinearGradient(
                        colors: palette.gradient,
                        begin: Alignment.topLeft,
                        end: Alignment.bottomRight,
                      ),
                      borderRadius: BorderRadius.circular(24),
                      boxShadow: const [
                        BoxShadow(
                          color: Color(0x22000000),
                          blurRadius: 24,
                          offset: Offset(0, 10),
                        ),
                      ],
                    ),
                    child: InkWell(
                      onTap: _dismiss,
                      borderRadius: BorderRadius.circular(24),
                      child: Padding(
                        padding: const EdgeInsets.fromLTRB(16, 14, 12, 14),
                        child: Row(
                          children: [
                            Container(
                              width: 42,
                              height: 42,
                              decoration: BoxDecoration(
                                color: Colors.white.withValues(alpha: 0.16),
                                borderRadius: BorderRadius.circular(14),
                              ),
                              child: Icon(
                                palette.icon,
                                color: Colors.white,
                                size: 22,
                              ),
                            ),
                            const SizedBox(width: 12),
                            Expanded(
                              child: Text(
                                widget.message,
                                style: Theme.of(context).textTheme.bodyMedium
                                    ?.copyWith(
                                      color: Colors.white,
                                      fontWeight: FontWeight.w600,
                                    ),
                              ),
                            ),
                            IconButton(
                              onPressed: _dismiss,
                              icon: const Icon(Icons.close_rounded),
                              color: Colors.white.withValues(alpha: 0.9),
                              tooltip: 'Fechar',
                            ),
                          ],
                        ),
                      ),
                    ),
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}
