import json
import queue
from tqdm import tqdm
from generate_graph import Graph

"""
# ============== NOTES ===============
3 start moves - each addition adds 2 (and does not cost a move)

13 max length = 21 max move bank

Moves are zero indexed with the 0th move being the start word (assigned)
For a given length of word:
    It is first possible to see on move `4 - length`
    The last possible time you can play it is with move `3 + 2 * (length - 4)`

gain score equal to the length of source word - NOT destination word
"""

MIN_LENGTH = 4
MAX_LENGTH = 13
STARTING_MOVES = 3
ADDITION_GAINED_MOVES = 2

class SolvedNode:
    def __init__(self, node):
        self.node = node
        self.tier = len(self.node.word) - MIN_LENGTH
        
        # Whether or not this node can be reached from a shorter word or
        # if it is a starting word. Will be initialized later for non-starting words
        self.is_entry = self.tier == 0
        
    def init_max_moves_left(self, max_moves):
        self.max_moves_on_entry = max_moves
        self.optimal_scores = [0] * max_moves
        self.optimal_paths = [[]] * max_moves
    
    def to_serializable(self):
        return {
            'id': self.node.index,
            'word': self.node.word,
            'tier': self.tier,
            'max_moves_on_entry': self.max_moves_on_entry,
            'children': [child.index for child in self.node.children],
            'is_entry': self.is_entry,
        }

if __name__ == '__main__':
    graph = Graph.from_file('graph.json')

    total_tiers = MAX_LENGTH - MIN_LENGTH + 1
    solved_by_tier = [[] for _ in range(total_tiers)]
    solved_by_id = {}
    
    for node in tqdm(graph.nodes, desc='Importing graph'):
        solved = SolvedNode(node)
        solved_by_tier[solved.tier].append(solved)
        solved_by_id[node.index] = solved

    # We only perform exhaustive search on tier entry points (nodes that can be
    # reached via addition from a shorter word). Compute those now
    for id in tqdm(solved_by_id, desc='Finding tier entry points'):
        node = solved_by_id[id].node

        for child in node.children:
            if len(child.word) > len(node.word):
                solved_by_id[child.index].is_entry = True
    
    # Because we memoize the optimums for each combination of node + moves left, we
    # need to calculate the max possible moves you can have left when first reaching a node.
    max_move_queue = queue.Queue()
    max_move_visited = set()
    for start_node in solved_by_tier[0]:
        max_move_queue.put((start_node, STARTING_MOVES))

    # BFS to find shortest path to each node
    while not max_move_queue.empty():
        (solved, moves) = max_move_queue.get() 
        if solved.node.index in max_move_visited:
            continue

        max_move_visited.add(solved.node.index)
        solved.init_max_moves_left(moves)
        for child in solved.node.children:
            if len(child.word) > len(solved.node.word):
                # If this is an addition, we gain moves
                max_move_queue.put((solved_by_id[child.index], moves + ADDITION_GAINED_MOVES))
            else:
                # Otherwise we lose a move
                max_move_queue.put((solved_by_id[child.index], moves - 1))
     
    # Store the results of the first tier (4 letter words)
    with open('preprocessed_graph.json', 'w') as f:
        output_nodes = []

        for node in solved_by_id.values():
            output_nodes.append(node.to_serializable())
            
        json.dump(output_nodes, f)

                