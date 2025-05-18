# Solver

This directory contains stuff related to the optimal solver for Stumper (as described [here](https://csun.io/solving-stumper)).

If you want to browse the optimal solutions for all starting words, check out [optimums.json](./optimums.json).

You can run the solver with `cargo run --release` (you'll need rust installed). The source is in [src/main.rs](./src/main.rs).

The solver depends on some pregenerated word graphs, which are created and manipulated by the various python scripts in here. You can also run [query.py](./query.py) to look up the possible moves for any given word.
