import argparse
import random
import sys
from generate_graph import Graph, STARTING_LENGTH

class Game:
    def __init__(self, singleplayer):
        self.singleplayer = singleplayer
        self.current_player = 0
        self.current_node = None
        self.strikes = [0, 0]
        self.graph = Graph.from_file('graph.json')
        self.used_nodes = set()

    def valid_moves_from_node(self, node):
        return node.children.difference(self.used_nodes)

    def add_strike(self, message):
        print(message)

        self.strikes[self.current_player] += 1

        if self.strikes[self.current_player] >= 3:
            print(f'Player {self.current_player} has been Stumped.')
            if self.current_node is not None:
                valid_choices = self.valid_moves_from_node(self.current_node)
                valid_choices_str = ', '.join([node.word for node in valid_choices])
                print(f'Valid choices were [{valid_choices_str}]')

            sys.exit(0)
        else:
            print(f'\tYou have used {self.strikes[self.current_player]} / 3 strikes')

    def do_computer_turn(self):
        pass

    def do_player_turn(self):
        word = input(f'Player {self.current_player} enter a word: ').strip().upper()

        node = self.graph.query(word)

        if node is None:
            self.add_strike(f'\t{word} is not a valid word.')
            return
        if node in self.used_nodes:
            self.add_strike(f'\t{word} has already been played!')
            return
        if self.current_node is not None and node not in self.current_node.children:
            self.add_strike(f'\t{word} is not reachable from {self.current_node.word}.')
            return
        
        print(f'\tChose word {word} with popularity {node.popularity}')
        self.used_nodes.add(node)
        self.current_node = node
        self.current_player = (self.current_player + 1) % 2

        valid_choices = self.valid_moves_from_node(self.current_node)
        if len(valid_choices) == 0:
            print(f'Player {self.current_player} has been Stumped. No valid moves remaining.')
            sys.exit(0)
        else:
            winning_moves_count = 0
            highest_popularity = 0
            for node in valid_choices:
                highest_popularity = max(highest_popularity, node.popularity)
                if len(self.valid_moves_from_node(node)) == 0:
                    winning_moves_count += 1
            print(f'\tThere are {len(valid_choices)} possible moves with highest popularity {highest_popularity}. {winning_moves_count} are Stumpers.')


    def play(self):
        self.current_node = random.choices(
            population=self.graph.valid_start_nodes,
            weights=[node.popularity for node in self.graph.valid_start_nodes],
            k=1)[0]
        print(f'{self.current_node.word} is the initial word.')

        while True:
            if self.singleplayer and self.current_player == 1:
                self.do_computer_turn()
            else:
                self.do_player_turn()


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--singleplayer')
    args = parser.parse_args()

    game = Game(args.singleplayer)
    game.play()