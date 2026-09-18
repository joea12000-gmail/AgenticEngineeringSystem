#!/usr/bin/env bash
# Script to create a branch, commit local changes, push, and open a GitHub PR.
# Usage: ./scripts/create_pr.sh "feature/iterative-workflow-engine" "Iteratively process executable tasks in WorkflowEngine"

set -euo pipefail

BRANCH=${1:-feature/iterative-workflow-engine}
TITLE=${2:-"Iteratively process executable tasks in WorkflowEngine"}
BODY_FILE=${3:-docs/PRs/feature-iterative-workflow-engine.md}
REMOTE=${4:-origin}
MILESTONE="Tests passing and CI/CD yaml created"

# Ensure working tree is clean
if ! git diff --quiet || ! git diff --staged --quiet; then
  echo "You have unstaged or staged changes. Staging all changes for commit."
  git add .
fi

# Create and checkout branch
git checkout -B "$BRANCH"

# Commit
git commit -m "$TITLE" || echo "No changes to commit"

# Push branch
git push -u "$REMOTE" "$BRANCH"

# Open PR with gh if available, otherwise provide URL
if command -v gh >/dev/null 2>&1; then
  # get list of files changed in the commit
  CHANGED=$(git show --name-only --pretty="" HEAD | sed '/^$/d' | sed 's/^/ - /')

  PR_BODY=$'This PR adds:\n\n- azure-pipelines/azure-pipelines.yml\n- .github/workflows/azure-webapp-deploy.yml\n\nChanged files:\n'"$CHANGED"$'\n\nPlease replace placeholders (<your-app-service-name>, <Azure Service Connection Name>) and add the AZURE_CREDENTIALS secret for GitHub Actions.'

  # ensure milestone exists (create if missing)
  if ! gh milestone view "$MILESTONE" >/dev/null 2>&1; then
    echo "Milestone '$MILESTONE' not found, creating..."
    gh milestone create --title "$MILESTONE"
  fi

  # create or update PR associated with current branch and assign milestone
  if gh pr view --json url -q .url >/dev/null 2>&1; then
    gh pr edit --title "$TITLE" --body "$PR_BODY" --milestone "$MILESTONE"
  else
    gh pr create --title "$TITLE" --body "$PR_BODY" --label "ci" --milestone "$MILESTONE"
  fi
else
  REPO_URL=$(git config --get remote.$REMOTE.url)
  echo "gh CLI not found. Create a PR manually with this URL:" 
  echo "$REPO_URL/compare/main...$BRANCH?expand=1"
fi

# Run tests (optional)
if command -v dotnet >/dev/null 2>&1; then
  echo "Running unit tests..."
  dotnet test
fi

echo "Done. Branch: $BRANCH"