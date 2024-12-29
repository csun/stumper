import sys
from generate_graph import Graph

print(Graph.from_file('graph.json').query(sys.argv[1].strip().upper()))