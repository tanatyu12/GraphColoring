using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MainManager : MonoBehaviour {
	//ノード
	private GameObject[] nodes;
	//辺
	private GameObject[] edges;
	//座標からノードへのハッシュ
	private Dictionary<Vector3, GameObject> posToNodeHash;
	//ノード名からノードへのハッシュ
	private Dictionary<string, GameObject> nameToObjectHash;
	//各ノードの隣接ノードのハッシュ
	private Dictionary<string, List<string>> adjacentNodeHash;
	//各ノードの次数
	private int[] degArr;
	//次数が高い順にソートしたノードリスト
	private List<GameObject> nodesSortedByDeg;


	// Use this for initialization
	void Start () {
		nodes = GameObject.FindGameObjectsWithTag("Node");
		edges = GameObject.FindGameObjectsWithTag("Edge");
		degArr = new int[nodes.Length];

		MakePosToNodeHash();
		MakeAdjacentNodeHash();
		MakeSortedNodeList();
		StartCoroutine("GraphColoring");
	}

	//座標からノードへのハッシュテーブルを作成
	private void MakePosToNodeHash() {
		posToNodeHash = new Dictionary<Vector3, GameObject>();
		foreach (GameObject node in nodes) {
			posToNodeHash[node.transform.position] = node;
		}
	}

	//隣接ノードのハッシュテーブルを作成
	private void MakeAdjacentNodeHash() {
		adjacentNodeHash = new Dictionary<string, List<string>>();
		nameToObjectHash = new Dictionary<string, GameObject>();

		foreach (GameObject node in nodes) {
			string nodeName = node.name;
			nameToObjectHash[nodeName] = node;
			adjacentNodeHash[nodeName] = new List<string>();
			Vector3 nodePos = node.transform.position;
			
			foreach (GameObject edge in edges) {
				LineRenderer lineRenderer = edge.GetComponent<LineRenderer>();
				for (int i = 0; i < 2; i++) {
					if (nodePos == lineRenderer.GetPosition(i)) {
						int idx = (i == 0) ? 1 : 0;
						adjacentNodeHash[nodeName].Add(posToNodeHash[lineRenderer.GetPosition(idx)].name);
						degArr[int.Parse(node.name.Substring(4))-1] ++;
					}
				}
			}
		}
	}

	//次数が高い順にソートされたノードリストを作成
	private void MakeSortedNodeList() {
		nodesSortedByDeg = new List<GameObject>();
		for (int i = degArr.Max(); i >= 0; i--) {
			for (int j = 0; j < degArr.Length; j++) {
				if (degArr[j] == i) {
					degArr[j] = -1;
					nodesSortedByDeg.Add(nameToObjectHash["Node"+(j+1).ToString()]);
				}
			}
		}
	}

	//頂点彩色を実行(Wellsh-Pawellの頂点彩色アルゴリズム)
	IEnumerator GraphColoring() {
		var coloredNodes = new List<string>();
		var colors = new Queue<Color>();
		colors.Enqueue(Color.red);
		colors.Enqueue(Color.blue);
		colors.Enqueue(Color.yellow);
		colors.Enqueue(Color.green);
		var exceptNodes = new List<string>();

		while (coloredNodes.Count < nodes.Length) {

			yield return new WaitForSeconds(1.0f);

			Color color = colors.Dequeue();
			for (int i = 0; i < nodesSortedByDeg.Count; i++) {
				string nodeName = nodesSortedByDeg[i].name;
				if (!coloredNodes.Contains(nodeName) && !exceptNodes.Contains(nodeName)) {
					nodesSortedByDeg[i].GetComponent<Renderer>().material.color = color;
					coloredNodes.Add(nodesSortedByDeg[i].name);
					exceptNodes.AddRange(adjacentNodeHash[nodesSortedByDeg[i].name]);
				}

			}
			exceptNodes.Clear();
			
		}
	}

}
