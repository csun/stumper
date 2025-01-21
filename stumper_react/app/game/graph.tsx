const STARTING_WORD_LENGTH = 4;
const STARTING_WORD_MIN_POPULARITY = 3;

export type Node = {
  word: string;
  popularity: number;
  children: Node[];
};

export type RawNode = {
  word: string;
  popularity: number;
  children: number[];
};

export default class Graph {
  nodes: { [word: string]: Node };
  startingNodes: Node[];

  constructor(rawData: RawNode[]) {
    this.nodes = {};
    this.startingNodes = [];

    for (const rawNode of rawData) {
      this.nodes[rawNode.word] = {
        word: rawNode.word,
        popularity: rawNode.popularity,
        children: [],
      };
    }
    for (const rawNode of rawData) {
      for (const childIdx of rawNode.children) {
        this.nodes[rawNode.word].children.push(
          this.nodes[rawData[childIdx].word],
        );
      }
    }

    for (const node of Object.values(this.nodes)) {
      if (
        node.word.length == STARTING_WORD_LENGTH &&
        node.popularity >= STARTING_WORD_MIN_POPULARITY
      ) {
        this.startingNodes.push(node);
      }
    }
  }

  public query(word: string): Node | null {
    if (word in this.nodes) {
      return this.nodes[word];
    }

    return null;
  }

  // TODO this is not weighted
  public getRandomStartingNode(): Node {
    return this.startingNodes[
      Math.floor(this.startingNodes.length * Math.random())
    ];
  }
}
