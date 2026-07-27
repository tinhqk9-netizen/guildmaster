from typing import Dict, List, Set

class DependencyGraph:
    def __init__(self):
        self.edges = {} # node -> list of dependencies
        self.nodes = set()

    def add_dependency(self, node: str, depends_on: str):
        if node not in self.edges:
            self.edges[node] = []
        self.edges[node].append(depends_on)
        self.nodes.add(node)
        self.nodes.add(depends_on)

    def find_cycles(self) -> List[List[str]]:
        cycles = []
        visited = set()
        stack = []
        
        def dfs(curr):
            if curr in stack:
                cycle_start = stack.index(curr)
                cycles.append(stack[cycle_start:] + [curr])
                return
            if curr in visited:
                return
                
            visited.add(curr)
            stack.append(curr)
            
            for dep in self.edges.get(curr, []):
                dfs(dep)
                
            stack.pop()
            
        for node in self.nodes:
            if node not in visited:
                dfs(node)
                
        return cycles
