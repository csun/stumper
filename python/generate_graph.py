import json
from wordfreq import zipf_frequency

ALPHABET = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'
STARTING_LENGTH = 4
MIN_STARTING_POPULARITY = 2
ALLOW_DELETIONS = False
ALLOW_MOVES = False
ALLOW_ANAGRAMS = True

class Graph:
    def __init__(self):
        self.explore_queue = []
        self.word_to_node = {}
        self.anagram_to_nodes = {}
        self.nodes = []
        self.valid_start_nodes = []

    def from_file(filepath):
        graph = Graph()
        with open(filepath) as f:
            graph_contents = json.load(f)
        
        for word, popularity in zip(graph_contents['nodes'], graph_contents['popularities']):
            node = graph.add_word(word)
            node.popularity = popularity

        for parent, children in enumerate(graph_contents['edges']):
            for child in children:
                graph.nodes[parent].children.add(graph.nodes[child])

        graph.valid_start_nodes = [node for node in graph.nodes if
                                   len(node.word) == STARTING_LENGTH and
                                   len(node.children) > 0 and
                                   node.popularity >= MIN_STARTING_POPULARITY]

        return graph

    def query(self, word):
        if word in self.word_to_node:
            return self.word_to_node[word]
        return None

    def generate(self):
        with open('dictionary.txt') as wordlist:
            for word in wordlist.readlines():
                word = word.strip()
                node = self.add_word(word)
                if len(word) == STARTING_LENGTH:
                    node.enqueue_if_needed()
        print(f'Found {len(self.explore_queue)} starting nodes.')

        while len(self.explore_queue) > 0:
            node = self.explore_queue.pop()
            word = node.word

            for add_idx in range(len(word) + 1):
                for letter in ALPHABET:
                    node.try_connect(word[:add_idx] + letter + word[add_idx:])
            for edit_idx in range(len(word)):
                for letter in ALPHABET:
                    node.try_connect(word[:edit_idx] + letter + word[edit_idx + 1:])
            if ALLOW_DELETIONS:
                for remove_idx in range(len(word)):
                    node.try_connect(word[:remove_idx] + word[remove_idx + 1:])
            if ALLOW_MOVES:
                for letter in set(word):
                    for move_idx in range(len(word)):
                        node.try_connect(word[:move_idx] + letter + word[move_idx + 1:])
            if ALLOW_ANAGRAMS:
                for anagram_node in self.anagram_to_nodes[node.anagram_key]:
                    node.try_connect(anagram_node.word)

    def export(self, filepath):
        new_nodes = []
        
        # Remove nodes that were never queued (are not reachable)
        for node in self.nodes:
            if node.queued:
                new_nodes.append(node)
        
        for idx, node in enumerate(new_nodes):
            node.index = idx

        edges = []
        for node in new_nodes:
            node_edges = []
            for child in node.children:
                node_edges.append(child.index)
            edges.append(node_edges)    

        with open(filepath, 'w') as f:
            json.dump({'nodes': [node.word for node in new_nodes], 'popularities': [node.popularity for node in new_nodes], 'edges': edges}, f)
        
        print(f'Exported {len(new_nodes)} nodes.')

    def add_word(self, word):
        node = Node(self, word)
        self.word_to_node[word] = node
        self.nodes.append(node)

        if ALLOW_ANAGRAMS:
            if node.anagram_key not in self.anagram_to_nodes:
                self.anagram_to_nodes[node.anagram_key] = []
            self.anagram_to_nodes[node.anagram_key].append(node)

        return node

class Node:
    def __init__(self, graph, word):
        self.graph = graph
        self.word = word
        self.anagram_key = ''.join(sorted(word))
        self.children = set()
        self.index = 0
        self.popularity = zipf_frequency(word.lower(), 'en')
        self.queued = False
    
    def __str__(self):
        children = ', '.join(list(node.word for node in self.children))
        return f'{self.word}: {children}'

    def enqueue_if_needed(self):
        if not self.queued:
            self.graph.explore_queue.append(self)
            self.queued = True
    
    def try_connect(self, new_word):
        if new_word in self.graph.word_to_node:
            new_node = self.graph.word_to_node[new_word]
            if new_node == self:
                return
            self.children.add(new_node)
            new_node.enqueue_if_needed()

if __name__ == '__main__':
    graph = Graph()
    
    graph.generate()
    graph.export('graph.json')

    graph = Graph.from_file('graph.json')