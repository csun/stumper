import matplotlib.pyplot as plt
import numpy as np
import json

nodes = json.load(open('optimums.json'))

scores = []
moves = []
for node in nodes:
    word = node['word']
    score = node['optimal_score']
    optimal_path = node['optimal_path']
    move_count = len(optimal_path)

    scores.append(score)
    moves.append(move_count)
    if node['optimal_path'][-1] not in ['FORMALITIES', 'ORIENTALISTS']:
        print(f'Strange ending node: {word} ({score} score / {move_count} moves) {optimal_path}')

scores = np.array(scores)
moves = np.array(moves)

plt.hist(scores, bins=max(scores))
plt.title('Optimal Scores')
plt.savefig('plots/optimum_scores.png')

plt.clf()
plt.title('Optimal Move Counts')
plt.hist(moves, bins=max(moves))
plt.savefig('plots/optimum_moves.png')