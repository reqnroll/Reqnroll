# language: ht
Karakteristik: Keywords can be a prefix of another
  Some times keywords are a prefix of another keyword.
  In this scenario the parser should prefer the longest keyword.

  Senaryo: Erasing agent memory
    Sipoze ke there is agent J
    Ak there is agent K
    Le I erase agent K's memory
    Le sa a there should be agent J
    Men there should not be agent K
