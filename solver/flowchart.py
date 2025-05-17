import queue
import graphviz
import json

class FlowNode:
    def __init__(self, word):
        self.word = word
        self.prev_counts = {}
        self.occurrences = 0
    
    def prev_and_others(self, occurrence_threshold):
        prev = [word for word, count in sorted(self.prev_counts.items(), key=lambda x: x[1], reverse=True)
                if count >= occurrence_threshold]

        return prev[:2], [word for word, _ in self.prev_counts.items() if word not in prev[:2]]

nodes = json.load(open('optimums.json'))

flow_nodes = {}
for node in nodes:
    optimal_path = node['optimal_path']
    prev = None
    for word in optimal_path:
        if word not in flow_nodes:
            flow_nodes[word] = FlowNode(word)
        flow_nodes[word].occurrences += 1

        if prev is not None:
            flow_nodes[word].prev_counts[prev] = flow_nodes[word].prev_counts.get(prev, 0) + 1
        prev = word

graph = graphviz.Digraph('Move Flowchart') 

def graph_roots(roots, occurrence_threshold):
    explore = queue.Queue()
    for root in roots:
        explore.put(root)

    cached_others = {}
    cached_sorted_prev_count = {}
    visited = set()
    while not explore.empty():
        word = explore.get()
        if word in visited:
            continue

        visited.add(word)
        node = flow_nodes[word]

        if node.occurrences >= 1000:
            fontsize = '20'
        elif node.occurrences >= 300:
            fontsize = '16'
        else:
            fontsize = '12'

        graph.node(
            word,
            label=f'{word}\n{node.occurrences} Occurrences',
            shape='none',
            fontcolor='#2c8f82',
            fontsize=fontsize,
            penwidth='0')

        sorted_prev, others = node.prev_and_others(occurrence_threshold)
        for child in sorted_prev:
            graph.edge(word, child, dir='back')
            explore.put(child)

        cached_others[word] = others
        cached_sorted_prev_count[word] = len(sorted_prev)
    
    # Need to draw edges to "other" nodes after the fact so that we can
    # properly draw the ones to nodes that are already in the graph
    for word in visited:
        other_count = len(cached_others[word])
        for other in cached_others[word]:
            if other in visited:
                graph.edge(word, other, dir='back')
                other_count -= 1

        if cached_sorted_prev_count[word] > 0 and other_count > 0:
            label = 'And 1 other...' if others == 1 else f'And {len(others)} others...'
            graph.node(
                f'{word}_others',
                label=label,
                shape='none',
                fontcolor='#2c8f82',
                fontsize='8',
                penwidth='0')
            graph.edge(word, f'{word}_others', dir='back')
        
graph_roots(['FORMALITIES', 'ORIENTALISTS'], 80)
graph_roots(['PHRENOLOGIES'], 20)

graph.render(filename='flowchart', format='png', cleanup=True)

