use std::{
    collections::{HashMap, HashSet},
    fs::File,
    sync::Mutex,
};

use indicatif::{MultiProgress, ProgressBar, ProgressStyle};
use rand::seq::SliceRandom;
use rayon::prelude::*;
use serde::{Deserialize, Serialize};

const STARTING_MOVES: usize = 3;
const ADDITION_GAINED_MOVES: usize = 2;
// Mixes up the order of children so that, in theory, we might see greater variety
// in the paths we take.
const PERFORM_SHUFFLE: bool = true;

#[derive(Deserialize, Debug)]
struct Node {
    id: usize,
    word: String,
    tier: usize,
    max_moves_on_entry: i32,
    children: Vec<usize>,
    is_entry: bool,
}

impl Node {
    pub fn find_optimum(
        &self,
        moves_left: usize,
        all_nodes: &Vec<Node>,
        visited: &mut HashSet<usize>,
        memo: &HashMap<(usize, usize), (usize, Vec<usize>)>,
    ) -> (usize, Vec<usize>) {
        // Base case - no moves left
        if moves_left <= 0 {
            return (0, vec![self.id]);
        }

        visited.insert(self.id);

        let mut found_move = false;
        let mut optimal_score = 0;
        let mut optimal_path = vec![];

        for child_id in &self.children {
            if visited.contains(&child_id) {
                continue;
            }

            let child = &all_nodes[*child_id];
            let (child_score, child_path) = if child.word.len() > self.word.len() {
                // Addition case - we can use the memo table because we're crossing a tier boundary.
                // Note that we explicitly cannot use the memo table when searching within the same tier,
                // because it does not respect the visitation status of existing nodes. When we cross a
                // tier boundary we are guaranteed a clean slate because there is no chance we've already
                // used any nodes in the next tier.
                memo.get(&(child.id, moves_left + ADDITION_GAINED_MOVES))
                    .unwrap()
                    .clone()
            } else {
                // Normal case - use one move
                child.find_optimum(moves_left - 1, all_nodes, visited, memo)
            };

            // Include the no found move case here because dead ends will return a score of 0. Therefore
            // if there's only dead end continuations (like at the end of almost all paths) we will fail
            // to add the final move in the path.
            if !found_move || child_score > optimal_score {
                optimal_score = child_score;
                optimal_path = child_path;
            }
            found_move = true;
        }

        visited.remove(&self.id);

        // Note that there are cases where we have not found a single move (all children have been visited,
        // no children, etc.) and therefore we do not want to score here.
        if found_move {
            optimal_score += self.word.len();
        }
        optimal_path.push(self.id);

        (optimal_score, optimal_path)
    }
}

#[derive(Serialize, Debug)]
struct SolvedNode {
    word: String,
    optimal_score: usize,
    optimal_path: Vec<String>,
}

fn main() {
    let nodes: Vec<Node> =
        serde_json::from_reader::<File, Vec<Node>>(File::open("preprocessed_graph.json").unwrap())
            .unwrap()
            .into_iter()
            .map(|mut node| {
                // Randomly shuffle the children of each node to add some variety to the paths we take.
                if PERFORM_SHUFFLE {
                    let mut rng = rand::rng();
                    node.children.shuffle(&mut rng);
                }
                node
            })
            .collect();
    let mut nodes_by_tier: Vec<Vec<&Node>> = vec![vec![]];

    for node in nodes.iter() {
        if node.tier as usize >= nodes_by_tier.len() {
            nodes_by_tier.resize(node.tier as usize + 1, vec![]);
        }
        nodes_by_tier[node.tier as usize].push(node);
    }

    let multiprogress = MultiProgress::new();
    let optimums = Mutex::new(HashMap::new());
    for (index, tier) in nodes_by_tier.iter().rev().enumerate() {
        let progress = multiprogress.add(ProgressBar::new(tier.len() as u64));
        let progress_style = ProgressStyle::with_template(
            "[{elapsed_precise}] {bar:40.cyan/blue} {pos:>7}/{len:7} {msg}",
        )
        .unwrap();
        progress.set_style(progress_style);
        progress.set_message(format!(
            "Processing tier {}",
            nodes_by_tier.len() - index - 1
        ));

        let cloned_optimums = optimums.lock().unwrap().clone();

        tier.par_iter().for_each(|node| {
            // If this node is not an entry point, we only need to process it when traversing
            // from entry points on this tier. Some nodes can have negative max_moves_on_entry,
            // which indicates that they are not reachable from any start word using the given
            // move gain rules.
            if !node.is_entry || node.max_moves_on_entry < 0 {
                return;
            }

            let mut visited = HashSet::new();

            let min_moves = if node.tier == 0 { STARTING_MOVES } else { 1 };
            let max_moves = node.max_moves_on_entry as usize;
            for moves_left in min_moves..=max_moves {
                let (optimal_score, optimal_path) =
                    node.find_optimum(moves_left, &nodes, &mut visited, &cloned_optimums);
                optimums
                    .lock()
                    .unwrap()
                    .insert((node.id, moves_left), (optimal_score, optimal_path));
            }
            progress.inc(1);
        });

        progress.finish();
    }

    let locked_optimums = optimums.lock().unwrap();
    let output_nodes: Vec<SolvedNode> = nodes_by_tier[0]
        .iter()
        .map(|node| {
            let (score, id_path) = locked_optimums.get(&(node.id, STARTING_MOVES)).unwrap();
            let path = id_path
                .iter()
                .rev()
                .map(|id| nodes[*id].word.clone())
                .collect();
            SolvedNode {
                word: node.word.clone(),
                optimal_score: *score,
                optimal_path: path,
            }
        })
        .collect();

    serde_json::to_writer_pretty(File::create("optimums.json").unwrap(), &output_nodes).unwrap();
}
