import queue
import graphviz
import json

class FlowNode:
    def __init__(self, word):
        self.word = word
        self.child_counts = {}
        self.siblings = set()
        self.relevant_siblings = []
        self.occurrences = 0
        
    def register_path(self, optimal_path):
        self.occurrences += 1
        this_len = len(self.word)
        for path_word in optimal_path:
            if len(path_word) == this_len - 1:
                self.child_counts[path_word] = self.child_counts.get(path_word, 0) + 1
            if len(path_word) == this_len:
                self.siblings.add(path_word)

relationships_nodes = json.load(open('graph.json'))
true_parents = {}
for node in relationships_nodes:
    true_parents[node['word']] = [relationships_nodes[child]['word'] for child in node['children']]

nodes = json.load(open('optimums.json'))

flow_nodes = {}
for node in nodes:
    optimal_path = node['optimal_path']
    for word in optimal_path:
        if word not in flow_nodes:
            flow_nodes[word] = FlowNode(word)
        flow_nodes[word].register_path(optimal_path)

graph = graphviz.Digraph('Move Flowchart') 
explore = queue.Queue()
explore.put('FORMALITIES')
explore.put('PHRENOLOGIES')

visited = set()
while not explore.empty():
    word = explore.get()
    if word in visited:
        continue

    visited.add(word)
    node = flow_nodes[word]
    true_children = [(child, count) for child, count in node.child_counts.items if node.word in true_parents[child]]
    sorted_children = sorted(true_children, key=lambda x: x[1])

    # Combine so that we max out at 2 children and "other"
    filtered_children = []
    other_count = 0
    while len(sorted_children) > 0:
        child_word, child_count = sorted_children.pop()
        candidate = flow_nodes[child_word]

        if child_count < 40 or len(filtered_children) >= 2:
            other_count += 1
            continue

        relevant_siblings = []
        for sibling_word, sibling_count in sorted_children:
            if sibling_word in candidate.siblings:
                candidate.relevant_siblings.append(sibling_word)
                relevant_siblings.append((sibling_word, sibling_count))
                
        for sibling in relevant_siblings:
            sorted_children.remove(sibling)
        
        filtered_children.append((child_word, child_count))
    
    for child_word, count in filtered_children:
        graph.edge(word, child_word)
        explore.put(child_word)
    
    # Only add other node if we have at least one other child
    if len(filtered_children) > 1 and other_count > 0:
        id = f'{word}_other'
        graph.node(id, label=f'{other_count} Other Words')
        graph.edge(word, id)

    graph.node(word, label=f'{word}\n{node.occurrences} Occurrences')

graph.render(filename='flowchart', format='pdf', cleanup=True)

