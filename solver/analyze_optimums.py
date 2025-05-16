import matplotlib.pyplot as plt
import matplotlib.ticker as ticker
import numpy as np
import json

nodes = json.load(open('optimums.json'))

scores = []
moves = []
final_words = {}
used_by_length = {}
word_counts = {}
for node in nodes:
    word = node['word']
    score = node['optimal_score']
    optimal_path = node['optimal_path']
    move_count = len(optimal_path)

    scores.append(score)
    moves.append(move_count)
    if optimal_path[-1] not in final_words:
        final_words[optimal_path[-1]] = 0
    final_words[optimal_path[-1]] += 1

    for path_word in optimal_path:
        if path_word not in word_counts:
            word_counts[path_word] = 0
        if len(path_word) not in used_by_length:
            used_by_length[len(path_word)] = set()
        used_by_length[len(path_word)].add(path_word)
        word_counts[path_word] += 1

    if node['optimal_path'][-1] not in ['FORMALITIES', 'ORIENTALISTS', 'PHRENOLOGIES']:
        print(f'Strange ending node: {word} ({score} score / {move_count} moves) {optimal_path}')

total_counts_by_length = {}
edge_count_by_length = {}
base_graph = json.load(open('preprocessed_graph.json'))

# Some nodes are actually not reachable with a non-negative number of moves remaining, so don't consider those
# in our counts
ignore_nodes = {}
for node in base_graph:
    if node['max_moves_on_entry'] < 0:
        ignore_nodes[node['id']] = True


for node in base_graph:
    if node['id'] in ignore_nodes:
        continue

    node_len = len(node['word'])
    if node_len not in total_counts_by_length:
        total_counts_by_length[node_len] = 0
    if node_len not in edge_count_by_length:
        edge_count_by_length[node_len] = 0

    total_counts_by_length[node_len] += 1
    
    for child in node['children']:
        if child in ignore_nodes:
            continue
        edge_count_by_length[node_len] += 1
        
print(f'Ignored {len(ignore_nodes)} nodes with negative max_moves_on_entry')

scores = np.array(scores)
moves = np.array(moves)

final_words_zipped = [(word, count) for word, count in final_words.items()]
sorted_final_words = sorted(final_words_zipped, key=lambda x: x[1] * 10000 + len(x[0]), reverse=True)
filtered_common_words = [(word, count) for word, count in word_counts.items() if len(word) < 7 and count > 40]
sorted_common_words = sorted(filtered_common_words, key=lambda x: x[1], reverse=True)

word_lengths_sorted = sorted(total_counts_by_length.keys())
words_used_by_length = []
words_unused_by_length = []
words_used_percentage = []
avg_edge_counts = []
for length in word_lengths_sorted:
    if length not in used_by_length:
        used_by_length[length] = set()
    
    used_count = len(used_by_length[length])
    words_used_by_length.append(used_count)
    words_used_percentage.append(f'{used_count / total_counts_by_length[length]:.0%}')
    words_unused_by_length.append(total_counts_by_length[length] - used_count)
    print(f'Length {length}: {edge_count_by_length[length]} / {total_counts_by_length[length]}')
    avg_edge_counts.append(edge_count_by_length[length] / total_counts_by_length[length])

MAIN_COLOR = '#2c8f82'
SECONDARY_COLOR = '#b5e8e1'

fig = plt.figure(figsize=(10, 8))
ax = fig.add_subplot(1, 1, 1)
binwidth = 5
plt.hist(scores, bins=range(min(scores), max(scores) + binwidth, binwidth), color=MAIN_COLOR)
ax.set_title(f'Optimal Scores Histogram (Bin Width = {binwidth})')
ax.set_yscale('log')
ax.yaxis.set_major_formatter(plt.ScalarFormatter())
ax.set_ylabel('Occurrences (Log Scale)')
ax.set_xlabel('Optimal Score')
ax.xaxis.set_minor_locator(ticker.MultipleLocator(10))
plt.savefig('plots/optimal_scores.png')

plt.clf()
fig = plt.figure(figsize=(10, 8))
ax = fig.add_subplot(1, 1, 1)
moves_binned = np.bincount(moves)
plt.bar(range(0, max(moves)+1), moves_binned, color=MAIN_COLOR)
ax.set_title('Optimal Move Sequence Lengths')
ax.set_yscale('log')
ax.yaxis.set_major_formatter(plt.ScalarFormatter())
ax.set_ylabel('Occurrences (Log Scale)')
ax.set_xlabel('Optimal Move Sequence Length')
ax.xaxis.set_minor_locator(ticker.MultipleLocator(1))
plt.savefig('plots/optimal_moves.png')

plt.clf()
fig = plt.figure(figsize=(10, 8))
ax = fig.add_subplot(1, 1, 1)
plt.bar([word for word, _ in sorted_final_words], [count for _, count in sorted_final_words], color=MAIN_COLOR)
ax.set_title('Final Words in Optimal Move Sequence')
ax.set_yscale('log')
ax.yaxis.set_major_formatter(plt.ScalarFormatter())
ax.set_ylabel('Occurrences (Log Scale)')
ax.set_xlabel('Optimal Sequence Final Word')
ax.axes.set_xticklabels(ax.xaxis.get_majorticklabels(), rotation=-45, ha='left')
fig.tight_layout()
plt.savefig('plots/final_word_counts.png')

plt.clf()
fig = plt.figure(figsize=(10, 8))
ax = fig.add_subplot(1, 1, 1)
plt.bar([word for word, _ in sorted_common_words], [count for _, count in sorted_common_words], color=MAIN_COLOR)
ax.set_title('Most Common Words')
ax.yaxis.set_major_formatter(plt.ScalarFormatter())
ax.set_ylabel('Occurrences')
ax.set_xlabel('Word')
ax.axes.set_xticklabels(ax.xaxis.get_majorticklabels(), rotation=-45, ha='left')
fig.tight_layout()
plt.savefig('plots/common_words.png')

plt.clf()
fig = plt.figure(figsize=(10, 8))
ax = fig.add_subplot(1, 1, 1)
used_bar = ax.bar(word_lengths_sorted, words_used_by_length, label='Used', color=MAIN_COLOR)
ax.bar_label(used_bar, labels=words_used_percentage, label_type='center', color=SECONDARY_COLOR)
ax.bar(word_lengths_sorted, words_unused_by_length, label='Unused', bottom=words_used_by_length, color=SECONDARY_COLOR)
ax.set_title('Used and Unused Words in Optimal Sequence')
ax.set_yscale('log')
ax.yaxis.set_major_formatter(plt.ScalarFormatter())
ax.set_ylabel('Count (Log Scale)')
ax.set_xlabel('Word Length')
ax.xaxis.set_major_locator(ticker.MultipleLocator(1))
ax.legend(loc="upper right")
plt.savefig('plots/used_count.png')

plt.clf()
fig = plt.figure(figsize=(10, 8))
ax = fig.add_subplot(1, 1, 1)
plt.bar(word_lengths_sorted, avg_edge_counts, color=MAIN_COLOR)
ax.set_title('Average Possible Move Count')
ax.set_ylabel('Average Possible Move Count')
ax.set_xlabel('Word Length')
ax.xaxis.set_major_locator(ticker.MultipleLocator(1))
plt.savefig('plots/move_count.png')
