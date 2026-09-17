PR: feature/iterative-workflow-engine

Title: Ensure WorkflowEngine processes newly-executable tasks within a single run

Branch: feature/iterative-workflow-engine

Description:
This change updates WorkflowEngine.RunPendingWorkflowsAsync to iteratively re-evaluate and process executable tasks until none remain. Previously the engine computed a single snapshot of executable tasks per workflow and did not handle tasks that became executable as a result of earlier tasks executed within the same run. That caused human-assigned tasks to remain Pending instead of being Blocked until a subsequent run.

Changes:
- RunPendingWorkflowsAsync now processes tasks in batches and loops until no pending tasks with satisfied dependencies remain.
- Added comments explaining behavior and a SaveChanges between batches to persist progress.

Testing:
- Updated behavior makes the existing unit test (Orchestrator_respects_dependencies_and_approval_gate) pass: taskA runs and completes, taskB is detected in the next batch and marked Blocked in the same invocation.

Notes:
- This is bounded because only Pending tasks with satisfied dependencies are selected each iteration.
- Consider additional safeguards if workflows can create new Pending tasks indefinitely during execution.

Please create branch 'feature/iterative-workflow-engine', commit the changes, and open a PR with this description.
