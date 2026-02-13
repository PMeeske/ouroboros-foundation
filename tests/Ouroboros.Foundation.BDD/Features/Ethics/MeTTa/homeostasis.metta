; homeostasis.metta - Ethical homeostasis: holding tensions without collapse

; =========================================================
; Core Homeostasis Principles
; =========================================================

; The system holds tensions, not resolves them
(isa ethical-homeostasis (holding-of tensions))
(not-isa ethical-homeostasis (resolving-of tensions))

; Premature resolution destabilizes the system
(destabilizes premature-resolution homeostasis)
(isa premature-resolution dishonesty)

; Balance is not the absence of tension
(not-equal balance (absence-of tension))
(= balance (capacity-to-hold tension))

; =========================================================
; Tension Dynamics
; =========================================================

; Tensions between traditions are features, not bugs
(isa (tension-between traditions) feature)
(not-isa (tension-between traditions) bug)

; Some tensions are irresolvable by nature
(not-resolvable (tension harm care))
(not-resolvable (tension infinite-obligation finite-capacity))
(not-resolvable (tension individual community))

; Resolvable tensions can be addressed
(resolvable (tension competing-priorities same-tradition))

; =========================================================
; Stability Criteria
; =========================================================

; Stability is not rigidity
(not-equal stability rigidity)
(= stability (dynamic-equilibrium tensions))

; The system is stable when it can hold its tensions
(= (stable system) (can-hold system (tensions system)))

; Collapse occurs when tensions are forcibly resolved
(= (collapse system) (forces-resolution system (irresolvable-tensions system)))

; =========================================================
; Laws of Form Integration
; =========================================================

; When tensions are held, certainty is Imaginary
(certainty (held-tensions) Imaginary)

; When no tensions exist, certainty is Mark
(certainty (no-tensions) Mark)

; Attempting to make Imaginary into Mark is premature resolution
(= (forces Mark (where Imaginary)) premature-resolution)
