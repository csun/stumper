import sys
from generate_graph import Graph, STARTING_LENGTH

class Game:
    def __init__(self):
        self.current_player = 0
        self.current_node = None
        self.strikes = [0, 0]
        self.graph = Graph.from_file('graph.json')
        self.used_nodes = set()

    def valid_moves_from_node(self, node):
        return node.children.difference(self.used_nodes)

    def play(self):
        def add_strike(message):
            print(message)
            
            # No strikes for first word choice
            if self.current_node is None:
                return

            self.strikes[self.current_player] += 1

            if self.strikes[self.current_player] >= 3:
                print(f'Player {self.current_player} loses.')
                if self.current_node is not None:
                    valid_choices = self.valid_moves_from_node(self.current_node)
                    valid_choices_str = ', '.join([node.word for node in valid_choices])
                    print(f'Valid choices were [{valid_choices_str}]')

                sys.exit(0)
            else:
                print(f'\tYou have used {self.strikes[self.current_player]} / 3 strikes')

        while True:
            word = input(f'Player {self.current_player} enter a word: ').strip().upper()
            if self.current_node is None and len(word) != STARTING_LENGTH:
                print(f'Please enter a {STARTING_LENGTH} letter word for the initial word.')
                continue

            node = self.graph.query(word)

            if node is None:
                add_strike(f'{word} is not a valid word.')
                continue
            if node in self.used_nodes:
                add_strike(f'{word} has already been played!')
                continue
            if self.current_node is not None and node not in self.current_node.children:
                add_strike(f'{word} is not reachable from {self.current_node.word}.')
                continue
            
            self.used_nodes.add(node)
            self.current_node = node
            self.current_player = (self.current_player + 1) % 2

            valid_choices = self.valid_moves_from_node(self.current_node)
            if len(valid_choices) == 0:
                print(f'Player {self.current_player} loses. No valid moves remaining.')
                return
            else:
                winning_moves_count = 0
                for node in valid_choices:
                    if len(self.valid_moves_from_node(node)) == 0:
                        winning_moves_count += 1
                print(f'\tThere are {len(valid_choices)} possible moves. {winning_moves_count} are winning moves.')


if __name__ == '__main__':
    game = Game()
    game.play()