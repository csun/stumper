import sys
from generate_graph import Graph

graph = Graph.from_file('graph.json')
node = graph.word_to_node.get(sys.argv[1].strip().upper())

children = ', '.join(list(child.word for child in node.children))
parents = ', '.join(list(parent.word for parent in graph.nodes if (len(parent.word) < len(node.word)) and (node in parent.children)))

print(f'Word: {node.word}\nChildren: {children}\nAddition Parents: {parents}')