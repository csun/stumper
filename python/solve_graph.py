import json
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

# The max possible number of moves that a player can have left
# upon reaching a node at this tier
def max_moves_left(tier):
    return STARTING_MOVES + (ADDITION_GAINED_MOVES * tier)

class SolvedNode:
    def __init__(self, node):
        self.node = node
        self.tier = len(self.node.word) - MIN_LENGTH

        max_moves = max_moves_left(self.tier)
        self.optimal_scores = [0] * max_moves
        self.optimal_paths = [[]] * max_moves
        
    def optimum_for_moves_left(self, moves_left):
        if moves_left == 0:
            return 0, [self.node]
        
        # Moves left is 1-indexed whereas our array is 0-indexed. Subtract 1
        return self.optimal_scores[moves_left - 1], self.optimal_paths[moves_left - 1]

    def set_optimum_for_moves_left(self, moves_left, score, path):
        # Moves left is 1-indexed whereas our array is 0-indexed. Subtract 1
        self.optimal_scores[moves_left - 1] = score
        self.optimal_paths[moves_left - 1] = path
    
    def to_serializable(self):
        return {
            'word': self.node.word,
            'optimal_scores': self.optimal_scores,
            'optimal_paths': [[node.word for node in path] for path in self.optimal_paths]
        }

def find_optimal(node, moves_left, solved_by_id, visited):
    if moves_left <= 0:
        return 0, [node]

    visited.add(node.index)

    optimal_score = 0
    optimal_path = []
    for child in node.children:
        if child.index in visited:
            continue

        child_solved = solved_by_id[child.index]
        # In the case of an addition, check for our memoized solution
        if len(child.word) > len(node.word):
            child_score, child_path = child_solved.optimum_for_moves_left(moves_left + ADDITION_GAINED_MOVES)
        else:
            child_score, child_path = find_optimal(child, moves_left - 1, solved_by_id, visited)
            
        if child_score > optimal_score:
            optimal_score = child_score
            optimal_path = child_path

    visited.remove(node.index)

    return optimal_score + len(node.word), [node] + optimal_path

if __name__ == '__main__':
    graph = Graph.from_file('graph.json')

    total_tiers = MAX_LENGTH - MIN_LENGTH + 1
    solved_by_tier = [[] for _ in range(total_tiers)]
    solved_by_id = {}
    
    for node in tqdm(graph.nodes, desc='Importing graph'):
        solved = SolvedNode(node)
        solved_by_tier[solved.tier].append(solved)
        solved_by_id[node.index] = solved

    # Iterate backwards starting with longest words
    for tier in range(total_tiers-1, -1, -1):
            for solved in tqdm(solved_by_tier[tier], desc=f'Processing tier {tier}'):
                for moves_left in range(1, max_moves_left(tier) + 1):
                    optimal_score, optimal_path = find_optimal(solved.node, moves_left, solved_by_id, set())
                    solved.set_optimum_for_moves_left(moves_left, optimal_score, optimal_path)

    # Store the results
    with open('optimal.json', 'w') as f:
        output_nodes = []

        for start_node in graph.valid_start_nodes:
            output_nodes.append(solved_by_id[start_node.index].to_serializable())
            
        json.dump(output_nodes, f, indent=2)

                