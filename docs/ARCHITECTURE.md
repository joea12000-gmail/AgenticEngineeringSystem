# Agentic Engineering System — Architecture

This document summarizes the architecture implemented in the POC and the recent additions (workflow engine, approval endpoints, tests).

Components

- API Host (ASP.NET Core minimal API)
- Persistence: EF Core with an open shared SQLite in-memory connection (configurable in DependencyInjection)
- Domain: URL shortener domain models (ShortUrl, UrlVisit)
- Orchestration: EngineeringWorkflow, EngineeringTask, EngineeringTaskDependency (stored in the DB)
- Workflow Engine: a small orchestrator that evaluates ready tasks, honors approval gates, and persists audit events
- Governance: a small deterministic PolicyEngine that classifies actions as Allowed / RequiresApproval / Denied
- Tests: unit test validating dependency ordering and approval gate using in-memory SQLite

Flow overview

1. A workflow is created and persisted with tasks and explicit dependencies.
2. The WorkflowEngine scans pending workflows, identifies executable tasks whose dependencies are satisfied and either executes them or sets them to blocked when they require human approval.
3. Human approvers call the approval endpoints to allow blocked tasks to proceed (or to reject them). Approvals and rejections are recorded as AuditEvent entries.
4. The engine persists task and workflow state and records audit events for traceability.

Notes and limitations

- This is a starter implementation focused on demonstrating orchestration concepts: it intentionally leaves agent execution as a placeholder.
- Production concerns (authentication/authorization, durable DB, telemetry, rollback handlers, policy enforcement integration) are documented as next steps and must be implemented for a hardened system.
