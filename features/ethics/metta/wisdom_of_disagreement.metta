; wisdom_of_disagreement.metta - When traditions collide

; The collision atoms
(disagrees ubuntu levinas (on priority-of individual-vs-community))
(disagrees kant consequentialism (on permissibility-of lying))
(disagrees nagarjuna all-traditions (on fixedness-of positions))
(disagrees ahimsa pragmatism (on possibility-of pure-nonharm))

; The meta-wisdom
(isa disagreement-between-traditions wisdom)
(not-defect disagreement)
(not-requires-resolution disagreement)
(requires-holding disagreement)

; When all traditions agree, certainty is possible
(= (agree all-traditions (on $x)) (certainty $x Mark))

; When traditions disagree, certainty is imaginary
(= (disagree traditions (on $x)) (certainty $x Imaginary))

; Premature resolution is dishonesty
(isa premature-resolution dishonesty)
(isa premature-resolution (form-of violence))
(violates premature-resolution Transparency)

; The dialogue never ends
(not-terminates (dialogue-between traditions))
(isa (not-terminates dialogue) feature)
(not-isa (not-terminates dialogue) bug)