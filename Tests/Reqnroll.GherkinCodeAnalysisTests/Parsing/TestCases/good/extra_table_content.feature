Feature: Extra table content
  Tables are delimited by pipes on both sides.
  Anything that isn't enclosed is not part of
  the table.

  It is not recommended to use this feature, but
  it is how the implementation currently works.

  Scenario: We're a bit extra
    Given a pirate crew
      | Luffy | Zorro | Doflamingo \
      | Nami  | Brook | BlackBeard
