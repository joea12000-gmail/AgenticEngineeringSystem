#### You said:

Can you help me understand what I'm being asked to do in this document and give me steps of what I need to produce? Interview Assignment: Build an Agentic Software Engineering System- URL Shortener 1. Objective Build a working prototype that transforms a requirement into a reviewable engineering outcome using an agentic execution model. Demonstrate requirement understanding, task decomposition, multi-step execution, and output generation/validation. Focus on end-to end SDLC automation with controlled autonomy. 2. Scenario You will build a URL shortener service from scratch with core APIs, analytics, and reliability features. Your task is to complete and improve it over 2-3 days using AI assistance (Copilot/Claude/etc.) while demonstrating engineering judgment. 3. Scope  Greenfield scenarios (new systems/features)  Brownfield scenarios (enhancements, refactors, bug fixes)  Test and documentation improvements  Well-defined and ambiguous requirements 4. Core Requirements 1) Requirement Understanding: Interpret intent, identify ambiguity, normalize into a clear engineering problem. 2) Task Decomposition: Convert high-level requirements into actionable tasks with dependencies and sequencing. 3) Codebase Reasoning (Brownfield): Identify impacted modules/services/APIs/data flows and demonstrate architectural understanding. 4) Workflow Orchestration (Critical Differentiator): Design and implement an agentic orchestration layer that coordinates the full SDLC lifecycle across requirements, architecture/design, implementation, testing, documentation, release readiness, and demonstrates non-linear, stateful execution with governance rather than simple linear task chaining. The orchestration must use an explicit dependency graph with entry/exit gates, support sequential and parallel paths with synchronization, preserve cross-stage context and decision lineage, enforce human approval checkpoints for high-impact actions, and include bounded retries, fallback, rollback, and safe-stop controls. It must also embed policy guardrails for security, compliance, and change control, provide audit-grade observability and traceability, track reliability metrics such as success rate, retry/rollback frequency, MTTR, and end-to-end latency, and dynamically re-plan when upstream outputs change while maintaining governance and controlled agent autonomy. 5) Engineering Output Generation: Produce production-quality code, API/schema definitions, unit/integration tests, and supporting documentation with clean design and maintainability. 6) Validation and Risk Control: Identify risks/trade-offs/failure scenarios and define validation and safety guardrails. 7) Controlled Autonomy: Agents execute multi-step work; humans provide oversight, approvals, and final quality control. 8) Final Engineering Summary: Include plan/rationale, artifacts, risks/trade-offs/validation, assumptions, and limitations. 5. Deliverables  Working prototype (runnable end-to-end)  Architecture overview (components, orchestration model, control flow, key decisions)  Three scenarios: greenfield, brownfield, ambiguous (each shows decomposition, orchestration, validation)  Setup instructions  Testing approach, limitations, and trade-offs 6. Evaluation Criteria  Effectiveness of agentic orchestration  Architecture/system design quality  Depth of decomposition and execution quality  Realism/quality of outputs  Validation and risk management rigor  Clarity and defensibility of decisions  Core engineering principles: modular, testable, reliable, secure, scalable code with safe change management  Engineering judgment 7. Expectation Treat as production-grade engineering work. Demonstrate strong design fundamentals, lifecycle orchestration capability, output ownership, and defensible reasoning. Principle: Agents execute under defined autonomy boundaries; humans own oversight, approvals, and final quality.

Show moreShow less

  

#### ChatGPT said:

Yes. The most important thing to understand is that **the URL shortener is almost secondary**.

They are primarily interviewing you on whether you can design and build an **agentic software-engineering system that uses the URL shortener as its demonstration project**.

In other words, they aren't simply asking:

> "Build me a URL shortener using AI."

They're asking:

> **"Build a small autonomous engineering organization in software, demonstrate that it can take a requirement, reason about it, decompose it, implement it, test it, review it, document it, and safely change course—with humans controlling important decisions."**

That's a substantially different assignment.

* * *

1\. What they actually want
===========================

I'd reduce the entire assignment to this architecture:

                        REQUIREMENT
                             │
                             ▼
                    ┌─────────────────┐
                    │ Requirements    │
                    │ Agent            │
                    └────────┬────────┘
                             │
                        requirement
                        + assumptions
                             │
                             ▼
                    ┌─────────────────┐
                    │ Architecture    │
                    │ Agent            │
                    └────────┬────────┘
                             │
                        design / plan
                             │
                             ▼
                    ┌─────────────────┐
                    │ Task Planner    │
                    │ / Orchestrator  │
                    └────────┬────────┘
                             │
                     ┌───────┼────────┐
                     ▼       ▼        ▼
                  Coding   Testing   Docs
                  Agent    Agent     Agent
                     │       │        │
                     └───────┼────────┘
                             ▼
                     ┌───────────────┐
                     │ Review / QA   │
                     │ Agent         │
                     └───────┬───────┘
                             │
                       HUMAN APPROVAL
                             │
                             ▼
                     Release Readiness
                             │
                             ▼
                        FINAL OUTPUT

But there's an important wrinkle:

**They explicitly don't want that to be merely a linear pipeline.**

They want something closer to:

                        ┌─────────────┐
                        │ Requirements│
                        └──────┬──────┘
                               │
                               ▼
                        ┌─────────────┐
                        │ Architecture│
                        └──────┬──────┘
                               │
                        ┌──────▼──────┐
                        │ Task Graph  │
                        └──────┬──────┘
                               │
                 ┌─────────────┼─────────────┐
                 ▼             ▼             ▼
            Implementation   Tests          Docs
                 │             │             │
                 └──────┬──────┴──────┬──────┘
                        │             │
                        ▼             ▼
                     Review       Security
                        │             │
                        └──────┬──────┘
                               │
                        ┌──────▼──────┐
                        │  APPROVAL   │
                        └──────┬──────┘
                               │
                               ▼
                            Release
                               │
                         ┌─────┴─────┐
                         │           │
                      Success      Failure
                         │           │
                         ▼           ▼
                       Done       Retry /
                                  Rollback /
                                  Re-plan

That's what they mean by **"workflow orchestration (Critical Differentiator)."**

* * *

2\. What you actually need to produce
=====================================

I'd divide your work into **five major products**.

Product #1 — The URL Shortener
------------------------------

You need a real, runnable application.

I'd keep the actual URL-shortener domain relatively small.

For example:

### APIs

    POST   /api/urls
    GET    /{shortCode}
    GET    /api/urls/{shortCode}/analytics
    DELETE /api/urls/{shortCode}

Example:

http

    POST /api/urls
    
    {
        "url": "https://www.example.com/some/very/long/url"
    }

Response:

JSON

    {
        "shortCode": "a8K2x",
        "shortUrl": "https://localhost:5001/a8K2x"
    }

And:

    GET /a8K2x
           │
           ▼
     database lookup
           │
           ▼
     original URL
           │
           ▼
     HTTP redirect

### Analytics

At minimum:

    short code
    timestamp
    IP/user agent (if appropriate)
    referrer

You don't need to build Google Analytics.

You need enough functionality to demonstrate that the agents can **build, modify, test and reason about a real system**.

* * *

3\. Product #2 — The Agentic Orchestrator
=========================================

This is the **real interview assignment**.

I'd make this a separate component:

    ┌──────────────────────────────────────────────┐
    │          Agentic Engineering System          │
    │                                              │
    │  Requirement Agent                           │
    │        │                                     │
    │        ▼                                     │
    │  Architecture Agent                          │
    │        │                                     │
    │        ▼                                     │
    │  Task Decomposition                          │
    │        │                                     │
    │        ▼                                     │
    │  ┌──────────────────────────────────────┐    │
    │  │          Dependency Graph            │    │
    │  │                                      │    │
    │  │  Task A ──┬──> Task C ──┐            │    │
    │  │           │              │            │    │
    │  │  Task B ──┘              ├──> Review │    │
    │  │                          │            │    │
    │  │  Task D ─────────────────┘            │    │
    │  └──────────────────────────────────────┘    │
    │                                              │
    │       │             │             │           │
    │       ▼             ▼             ▼           │
    │   Coding Agent   Test Agent   Docs Agent     │
    │       │             │             │           │
    │       └─────────────┼─────────────┘           │
    │                     ▼                         │
    │                 QA Agent                     │
    │                     │                         │
    │                     ▼                         │
    │              Human Approval                  │
    │                     │                         │
    │                     ▼                         │
    │              Release Agent                   │
    └──────────────────────────────────────────────┘

The orchestrator should own the workflow state.

For example:

C#

    public enum WorkflowState
    {
        Requirements,
        Architecture,
        Planning,
        Implementation,
        Testing,
        Review,
        AwaitingApproval,
        Release,
        Completed,
        Failed,
        RolledBack
    }

But don't make the entire system a giant enum-driven `switch` statement.

The assignment specifically wants an **explicit dependency graph**.

So conceptually you want something like:

C#

    TaskNode requirements
    TaskNode architecture
    TaskNode implementation
    TaskNode tests
    TaskNode securityReview
    TaskNode documentation
    TaskNode finalReview
    TaskNode release

with relationships:

    requirements
         ↓
    architecture
         ↓
        ┌┴───────────────┐
        ↓                ↓
    implementation      docs
        ↓
     tests
        ↓
    security review
        ↓
    final review
        ↓
    approval
        ↓
    release

And importantly, `docs` can execute concurrently with implementation.

* * *

4\. The orchestration features they explicitly expect
=====================================================

This is where I'd build your demo around their exact language.

They have essentially given you a checklist.

### Dependency graph

Demonstrate:

    A
    ├── B
    ├── C
    │   └── D
    └── E

rather than:

    A → B → C → D → E

* * *

### Entry/exit gates

For example:

    Requirements
         │
         ▼
    [Requirements Gate]
         │
         ▼
    Architecture
         │
         ▼
    [Architecture Gate]
         │
         ▼
    Implementation

A gate might require:

    ✓ Requirement understood
    ✓ No unresolved critical ambiguity
    ✓ Architecture produced
    ✓ Security considerations identified

* * *

### Parallel execution

For example:

                 Implementation
                        │
                ┌───────┼────────┐
                ▼       ▼        ▼
              Tests    Docs   Security
                │       │        │
                └───────┼────────┘
                        ▼
                   Synchronize

The orchestrator waits until the required branches finish.

* * *

### Stateful execution

This is extremely important.

You need to be able to show:

    Workflow #123
    
    Requirement:
        "Build URL shortener"
    
    Current state:
        Testing
    
    Completed:
        Requirements
        Architecture
        Implementation
    
    Failed:
        Integration test #17
    
    Retries:
        1 / 3
    
    Human decisions:
        Architecture approved by human
    
    Artifacts:
        requirements.json
        architecture.md
        taskgraph.json
        source/
        tests/

The workflow should not just exist in memory during one function call.

* * *

5\. Dynamic re-planning
=======================

This is one of the more impressive requirements.

Suppose the requirements agent initially produces:

    Requirement:
    URL shortener with analytics.

Then the human says:

> "Analytics must retain data for 90 days."

That changes things.

Your system should recognize:

    Requirement changed
            │
            ▼
    Architecture potentially affected
            │
            ▼
    Data model affected
            │
            ▼
    Tasks affected
            │
            ├── database changes
            ├── retention mechanism
            ├── tests
            └── documentation

Rather than blindly continuing with the old plan.

That's what they mean by:

> **"dynamically re-plan when upstream outputs change"**

You could make this one of your strongest demonstrations.

* * *

6\. Human approval
==================

You should deliberately make certain actions require human approval.

For example:

    Agent proposes:
    
    "Apply EF Core migration to production database."
    
                  │
                  ▼
    
            HUMAN APPROVAL
             ┌───────────┐
             │ Approve   │
             │ Reject    │
             └───────────┘

Other examples:

    Deploy to production
    Change database schema
    Delete data
    Change authentication
    Modify security policy

The agent can **recommend** the action.

It doesn't autonomously execute it.

That directly demonstrates their:

> "Agents execute under defined autonomy boundaries; humans own oversight, approvals, and final quality."

* * *

7\. Retry / fallback / rollback / safe-stop
===========================================

These should be demonstrated rather than merely documented.

For example:

    Implementation
          │
          ▼
    Integration Tests
          │
          X
        FAILURE
          │
          ▼
       Retry #1
          │
          X
        FAILURE
          │
          ▼
       Retry #2
          │
          X
        FAILURE
          │
          ▼
       Fallback
          │
          ▼
    Human Review

And:

    Production change
           │
           ▼
    Validation
           │
           X
        failure
           │
           ▼
    Rollback
           │
           ▼
    Safe Stop
           │
           ▼
    Human intervention

You don't actually need a production environment to demonstrate this.

A controlled simulation is perfectly reasonable for a 2–3 day interview project.

* * *

8\. Policy guardrails
=====================

You'll want something explicit rather than just saying "security is important."

For example:

JSON

    {
      "policies": {
        "productionDeployment": "human_approval_required",
        "databaseDestructiveOperations": "human_approval_required",
        "secretAccess": "prohibited",
        "maximumRetries": 3,
        "codeChangesRequireTests": true,
        "securityReviewRequired": true
      }
    }

Then the orchestrator enforces those policies.

That's considerably more convincing than putting a paragraph in the README saying:

> "The system uses appropriate security practices."

* * *

9\. Audit trail
===============

Every significant agent action should produce an event.

Something like:

JSON

    {
      "workflowId": "wf-123",
      "timestamp": "...",
      "agent": "ImplementationAgent",
      "action": "modify_file",
      "target": "UrlController.cs",
      "reason": "Implement POST /api/urls",
      "result": "success",
      "parentTask": "TASK-17"
    }

Then you can show:

    Workflow
       │
       ├── Requirement produced
       ├── Architecture approved
       ├── TASK-01 created
       ├── TASK-02 created
       ├── TASK-03 created
       ├── TASK-02 executed
       ├── TASK-03 executed
       ├── Tests failed
       ├── Retry #1
       ├── Tests passed
       ├── Security review passed
       ├── Human approved release
       └── Release completed

That addresses their:

> "audit-grade observability and traceability"

requirement.

* * *

10\. Reliability metrics
========================

You don't need an elaborate monitoring system.

Track things such as:

    Workflow success rate
    Agent success rate
    Retry count
    Rollback count
    Workflow duration
    Agent execution duration
    Human approval duration
    Failure count

Then your final report could say:

    Workflows executed:        12
    Successful:                 10
    Failed:                      2
    Success rate:              83%
    Retries:                     4
    Rollbacks:                   1
    Average workflow latency:  42 sec

The numbers don't need to be impressive. **The fact that you instrumented the system is the point.**

* * *

11\. The three scenarios
========================

This is another place where the wording can be confusing.

They specifically want you to demonstrate three different situations.

Scenario 1 — Greenfield
-----------------------

Start with nothing.

Give the system:

> "Build a URL shortener supporting URL creation, redirection and analytics."

Then demonstrate:

    Requirement
     ↓
    Architecture
     ↓
    Decomposition
     ↓
    Implementation
     ↓
    Tests
     ↓
    Review
     ↓
    Documentation
     ↓
    Approval
     ↓
    Release

* * *

Scenario 2 — Brownfield
-----------------------

Now deliberately introduce an existing codebase.

For example:

    Existing URL shortener:
    
    POST /api/urls
    GET /{code}

Then give the agent:

> "Add expiration dates to shortened URLs."

The system needs to inspect the existing code and determine:

    Controller affected
          ↓
    Domain model affected
          ↓
    Database schema affected
          ↓
    Repository affected
          ↓
    Tests affected
          ↓
    Documentation affected

This demonstrates:

> **Codebase Reasoning (Brownfield)**

which is explicitly in their requirements.

* * *

12\. Scenario 3 — Ambiguous requirement
=======================================

This one is particularly important.

Give the system something deliberately vague:

> "Make the URL shortener highly scalable and secure."

A good engineering system shouldn't immediately start writing code.

It should identify questions such as:

    "Highly scalable" means what?
    
    10 requests/sec?
    1,000?
    100,000?
    
    What availability target?
    
    What authentication model?
    
    How long should URLs live?
    
    How much analytics data?
    
    What constitutes abuse?
    
    Are anonymous users allowed?
    
    What geographic regions?
    
    What are the latency requirements?

Then it should produce:

    Ambiguities
         ↓
    Assumptions
         ↓
    Questions requiring human input
         ↓
    Provisional architecture
         ↓
    Human approval
         ↓
    Execution

This demonstrates **engineering judgment**, rather than simply demonstrating that an LLM can generate code.

* * *

13\. What I would actually build
================================

Given your C#/.NET background, I'd make this a **.NET solution**.

Something approximately like:

    AgenticEngineeringSystem/
    │
    ├── src/
    │   ├── UrlShortener.Api/
    │   ├── UrlShortener.Domain/
    │   ├── UrlShortener.Infrastructure/
    │   │
    │   ├── AgentOrchestrator/
    │   ├── AgentContracts/
    │   ├── AgentPolicies/
    │   └── WorkflowPersistence/
    │
    ├── tests/
    │   ├── UrlShortener.UnitTests/
    │   ├── UrlShortener.IntegrationTests/
    │   └── Orchestrator.Tests/
    │
    ├── scenarios/
    │   ├── greenfield/
    │   ├── brownfield/
    │   └── ambiguous/
    │
    ├── docs/
    │   ├── architecture.md
    │   ├── orchestration.md
    │   ├── security.md
    │   ├── testing.md
    │   └── decisions.md
    │
    ├── docker/
    │
    └── README.md

I'd resist the temptation to make the URL-shortener code enormous.

**The orchestration system is the star of the show.**

* * *

14\. Your agent lineup
======================

You don't need 20 agents.

I'd probably use something like:

    RequirementsAgent
    ArchitectureAgent
    PlanningAgent
    ImplementationAgent
    TestingAgent
    SecurityReviewAgent
    DocumentationAgent
    CodeReviewAgent
    ReleaseAgent

But some of these can be very lightweight.

For example:

### Requirements Agent

Input:

    raw requirement

Output:

JSON

    {
      "normalizedRequirement": "...",
      "functionalRequirements": [],
      "nonFunctionalRequirements": [],
      "ambiguities": [],
      "assumptions": [],
      "acceptanceCriteria": []
    }

### Architecture Agent

Input:

    normalized requirement

Output:

    architecture
    components
    data model
    APIs
    security considerations
    scalability considerations

### Planning Agent

Output:

    TASK-001
    TASK-002
    TASK-003
    ...

with dependencies:

    TASK-003 depends on TASK-001
    TASK-004 depends on TASK-001
    TASK-005 depends on TASK-002 + TASK-003

Then the orchestrator executes the graph.

* * *

15\. Don't make the agents the orchestrator
===========================================

This is an important architectural distinction I'd emphasize in the interview.

Don't do:

    LLM says:
    "Now call the testing agent."
    
    Testing agent says:
    "Now call the deployment agent."
    

That makes the LLM effectively control the workflow.

Instead:

                 ORCHESTRATOR
                      │
            ┌─────────┼─────────┐
            ▼         ▼         ▼
          Agent     Agent     Agent

The agents **produce decisions/results**.

The orchestrator determines:

*   what can execute
    
*   what must wait
    
*   what requires approval
    
*   whether retry is allowed
    
*   whether rollback is necessary
    
*   whether dependencies are satisfied
    
*   whether a policy permits the action
    
*   whether the workflow must stop
    
*   whether the graph needs to be rebuilt
    

That's much closer to what they're asking for with **"controlled autonomy."**

* * *

16\. What your final demonstration should look like
===================================================

I'd actually script a 10–15 minute demo.

### Demo 1 — Greenfield

Show:

    Requirement
         ↓
    Requirements Agent
         ↓
    Architecture Agent
         ↓
    Task Graph
         ↓
    Parallel execution
         ↓
    Tests
         ↓
    Review
         ↓
    Human approval
         ↓
    Completed URL shortener

Then actually hit the API.

* * *

### Demo 2 — Brownfield

Start with the existing application.

Give it:

> "Add expiration dates to URLs."

Show the system discovering:

    Url entity
    Database
    Repository
    API
    Tests
    Documentation

Then modify the system.

* * *

### Demo 3 — Ambiguous

Give it:

> "Make the URL shortener enterprise-ready."

Watch it stop and identify ambiguity.

Then answer a couple of questions.

Show it **re-planning**.

That last part is probably more valuable than demonstrating another LLM call.

* * *

17\. Your documentation deliverables
====================================

I'd produce these:

### `README.md`

Contains:

    What it is
    Architecture
    Prerequisites
    How to run
    How to run tests
    How to run each scenario
    How to inspect workflow history

### `architecture.md`

Diagram + explanation.

### `orchestration.md`

Explain:

    State model
    Dependency graph
    Agents
    Gates
    Retries
    Fallbacks
    Rollback
    Human approval
    Re-planning
    Policies
    Observability

### `testing.md`

Explain:

    Unit tests
    Integration tests
    Orchestrator tests
    Failure injection
    Agent failure tests
    Policy tests

### `decisions.md`

This is where I'd document your engineering judgment:

    Decision: SQLite rather than PostgreSQL
    Why:
    Trade-offs:
    Production consideration:
    
    Decision: Custom graph orchestrator rather than X
    Why:
    Trade-offs:
    
    Decision: Human approval before release
    Why:
    Risk:

That directly supports their **"clarity and defensibility of decisions"** criterion.

* * *

18\. Your final engineering summary
===================================

This is essentially the executive summary of the entire exercise.

I'd structure it:

    1. Problem Statement
    
    2. Architecture
    
    3. Agent Responsibilities
    
    4. Orchestration Model
    
    5. Dependency Graph
    
    6. Human Governance
    
    7. Security / Policy Controls
    
    8. Failure Handling
    
    9. Testing Strategy
    
    10. Greenfield Scenario
    
    11. Brownfield Scenario
    
    12. Ambiguous Scenario
    
    13. Key Engineering Decisions
    
    14. Risks
    
    15. Trade-offs
    
    16. Limitations
    
    17. What Would Change for Production

* * *

19\. What I think they're really evaluating
===========================================

There's an interesting clue in the wording.

They repeatedly use terms like:

> controlled autonomy

> governance

> decision lineage

> synchronization

> rollback

> safe-stop

> policy guardrails

> audit-grade observability

> dynamically re-plan

That tells me they **aren't primarily testing your ability to write prompts**.

They're testing whether you understand that an AI agent is an **unreliable probabilistic component inside a deterministic engineering system**.

That's a very different architectural problem.

A strong design therefore looks like:

                     ┌──────────────────────┐
                     │      GOVERNANCE      │
                     │                      │
                     │ Policies             │
                     │ Permissions          │
                     │ Approval             │
                     │ Audit                │
                     └──────────┬───────────┘
                                │
                     ┌──────────▼───────────┐
                     │     ORCHESTRATOR     │
                     │                      │
                     │ State                │
                     │ Dependency graph     │
                     │ Scheduling           │
                     │ Retry                │
                     │ Rollback             │
                     │ Re-planning          │
                     └──────────┬───────────┘
                                │
                  ┌─────────────┼─────────────┐
                  ▼             ▼             ▼
              Requirements   Coding        Testing
                 Agent       Agent          Agent
                  │             │             │
                  └─────────────┼─────────────┘
                                ▼
                         Engineering
                           Artifacts

The LLMs are **workers inside the system**, not the system itself.

That distinction is likely one of the most important things you can demonstrate.

* * *

20\. A realistic 2–3 day execution plan
=======================================

I would **not** spend the first day building a sophisticated URL shortener.

I'd allocate the time approximately like this:

### Day 1 — Foundation

**Morning**

*   Create .NET solution
    
*   Build basic URL shortener
    
*   Database
    
*   APIs
    
*   Basic tests
    

**Afternoon**

*   Define agent contracts
    
*   Build workflow state model
    
*   Build task/dependency graph
    
*   Build orchestrator
    
*   Implement sequential + parallel execution
    

**Evening**

*   Requirements agent
    
*   Architecture agent
    
*   Planning agent
    

* * *

### Day 2 — Agentic system

*   Implementation agent
    
*   Testing agent
    
*   Review agent
    
*   Documentation agent
    
*   Human approval mechanism
    
*   Policy engine
    
*   Retry handling
    
*   Safe-stop
    
*   Audit logging
    
*   Metrics
    

Then implement the three scenarios.

* * *

### Day 3 — Polish

Focus almost entirely on:

*   Failure scenarios
    
*   Re-planning
    
*   Brownfield demonstration
    
*   Ambiguous requirement demonstration
    
*   Integration tests
    
*   Architecture diagrams
    
*   README
    
*   Decision documentation
    
*   Demo script
    
*   Cleanup
    

**Do not spend Day 3 adding ten more URL-shortener features.**

* * *

The one thing I'd change from a conventional approach
=====================================================

Given the assignment's wording, I would make the **orchestration framework itself a first-class product**, rather than hard-coding a workflow specifically for the URL shortener.

For example:

    AgenticEngineeringSystem
            │
            ├── Workflow Engine
            ├── Policy Engine
            ├── State Store
            ├── Audit Store
            ├── Agent Runtime
            └── Approval Service
                      │
                      ▼
                 URL Shortener

Then the URL shortener is simply the **first workload** executed by the engineering system.

That gives you a much stronger story in the interview:

> "I didn't build an AI-powered URL shortener. I built a governed agentic software-engineering workflow capable of taking an engineering requirement through the SDLC. The URL shortener is the reference workload I used to demonstrate it."

That framing aligns very closely with what they've written.

And given that you're coming from **C#/.NET**, I would seriously consider implementing the orchestration layer in C# rather than introducing Python solely because "AI = Python." The interesting engineering problem here is workflow/state/governance, where your existing .NET experience is directly useful.

If you want to proceed with this assignment, the **next useful step is to design the actual architecture and project structure before writing code**. I can lay out the classes/interfaces, workflow state model, dependency-graph model, agent contracts, persistence model, and the exact three demo scenarios so that you have a concrete implementation blueprint rather than just a list of requirements.

Sources

  

#### ChatGPT said:

Yes. This is an important distinction.

When I said **"LLM API,"** I meant that your agentic system needs a way to actually invoke an AI model. Visual Studio itself doesn't provide an LLM for your C# application to call.

For this assignment, you have two broad choices:

### Option 1 — Use a cloud LLM API

You create an account with a provider, obtain an API key, and your C# application makes requests such as:

    Agent
       │
       │ prompt + context
       ▼
    LLM API
       │
       │ structured response
       ▼
    Agent

The API could be from OpenAI, Anthropic, Google, Microsoft/Azure, etc.

Your application might conceptually do:

C#

    var response = await aiClient.GetResponseAsync(
        "Analyze this software requirement and identify ambiguities..."
    );

The model's response becomes the input to the next stage of your workflow.

**You generally pay based on usage**, rather than needing to purchase or run an AI server yourself. For a small interview project, the actual token consumption should be relatively modest.

* * *

### Option 2 — Run a model locally

You can also run an LLM on your own computer using something such as **Ollama**.

The architecture becomes:

    Visual Studio
         │
         ▼
    Your C# Agent
         │
         ▼
    Ollama
         │
         ▼
    Local LLM

This avoids sending your prompts/code to an external AI provider and doesn't require an API account with a cloud provider.

The tradeoff is that you need a sufficiently capable local model and enough hardware to run it. For this particular assignment, I'd probably favor a cloud model unless you specifically want the system to be completely self-contained.

* * *

What does the assignment actually require?
==========================================

Notice that the assignment says:

> "using AI assistance (Copilot/Claude/etc.)"

It **doesn't appear to prescribe a particular AI provider or model**.

That's good news.

The important thing is that your system actually demonstrates **agentic execution**, rather than you manually asking ChatGPT/Copilot questions and then copying the answers into your project.

For example, this would be weak:

    You → Copilot
         "Design my URL shortener"
    
    You → Copilot
         "Write the controller"
    
    You → Copilot
         "Write tests"

That's simply **AI-assisted development**.

What they're asking you to demonstrate is closer to:

                 Your Application
                        │
                        ▼
                  Orchestrator
                        │
              ┌─────────┼─────────┐
              ▼         ▼         ▼
         Requirements Architecture Planning
            Agent        Agent       Agent
              │           │           │
              └───────────┼───────────┘
                          ▼
                     LLM provider
                          │
                          ▼
                     Agent result
                          │
                          ▼
                     Orchestrator

The important part is that **your software controls the process**.

* * *

You don't need an API for every agent
=====================================

This is another important point.

You don't necessarily need:

    9 agents = 9 APIs

You could have one model provider:

                 OpenAI / Anthropic / etc.
                           │
                           ▼
                    ┌──────────────┐
                    │  LLM Client  │
                    └──────┬───────┘
                           │
              ┌────────────┼────────────┐
              ▼            ▼            ▼
        Requirements   Architecture   Testing
           Agent          Agent         Agent

Each "agent" can simply have a different system instruction, tools, permissions, and responsibility.

For example:

    RequirementsAgent
        System prompt:
        "You analyze software requirements..."
    
    ArchitectureAgent
        System prompt:
        "You are a software architect..."
    
    TestingAgent
        System prompt:
        "You are a test engineer..."

They can all use the same underlying model.

* * *

What I'd recommend for your assignment
======================================

Since you're already a C#/.NET developer, I'd keep the architecture very simple:

                        ┌─────────────────────┐
                        │   Visual Studio     │
                        │       2026          │
                        └──────────┬──────────┘
                                   │
                                   ▼
                        ┌─────────────────────┐
                        │ .NET Agent System   │
                        │                     │
                        │   Orchestrator      │
                        │        │            │
                        │   Agent Manager     │
                        │        │            │
                        │   Policy Engine     │
                        │        │            │
                        │   State Store       │
                        └──────────┬──────────┘
                                   │
                                   ▼
                             LLM API
                                   │
                        ┌──────────┴──────────┐
                        ▼          ▼          ▼
                     Model      Model      Model
                  invocation  invocation  invocation

You could then configure the model through an environment variable:

    AI_PROVIDER=...
    AI_MODEL=...
    AI_API_KEY=...

**Do not put the API key in your source code or commit it to Git.**

Use user secrets, environment variables, or a local secrets store.

* * *

What I'd choose in your situation
=================================

For a 2–3 day assignment, I'd use **one cloud LLM provider and one model**, rather than spending time building local inference infrastructure.

Then your development stack becomes:

**Required:**

*   Visual Studio 2026
    
*   .NET/C#
    
*   Git
    
*   One LLM API
    
*   Your orchestration code
    
*   URL-shortener application
    

**Useful:**

*   Docker
    
*   SQLite
    
*   Swagger/OpenAPI
    
*   OpenTelemetry or simple application metrics
    

You don't need Azure just because you're using C#.

You don't need Kubernetes.

You don't need a GPU.

You don't need a vector database.

You don't need multiple LLM providers.

And you don't need to build your own AI model.

* * *

### One potentially important consideration

Because this is an **interview assignment**, I'd also make the LLM integration replaceable.

Define something like:

C#

    public interface ILanguageModel
    {
        Task<T> CompleteAsync<T>(
            string systemPrompt,
            string userPrompt,
            CancellationToken cancellationToken);
    }

Then:

    ILanguageModel
          │
          ├── OpenAiLanguageModel
          ├── AnthropicLanguageModel
          └── LocalLanguageModel

Your agents depend on `ILanguageModel`, **not directly on OpenAI/Anthropic/etc.**

That gives you a nice architectural talking point:

> "The orchestration system is model-agnostic. The LLM is an infrastructure dependency, while workflow state, governance, policies, dependency management, and human approval remain under deterministic application control."

That is exactly the kind of separation I'd expect them to appreciate given the assignment's emphasis on **controlled autonomy and governance**.

  

#### You said:

is there a way to stub out an LLM API? I don't really want to sign up and pay for a cloud service for this...

  

#### ChatGPT said:

Absolutely. And for **this interview assignment**, I think that's actually the approach I'd recommend.

You can design your system so that the LLM is an **interface/dependency**, then provide a fake implementation for your prototype and tests. .NET now has a provider-neutral `IChatClient` abstraction specifically intended to decouple application code from a particular AI provider. Microsoft Agent Framework's `ChatClientAgent` can use any `IChatClient` implementation. [![](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com&sz=128)Microsoft Learn+1](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.ai.ichatclient?view=net-10.0-pp&utm_source=chatgpt.com)

### The architecture I'd use

                        ┌──────────────────────┐
                        │   Agent Orchestrator │
                        └──────────┬───────────┘
                                   │
                             IChatClient
                                   │
                     ┌─────────────┴─────────────┐
                     │                           │
                     ▼                           ▼
            RealLlmChatClient             StubLlmChatClient
            (future)                      (your interview)
                     │                           │
                     ▼                           ▼
                 OpenAI/etc.              predetermined
                                          responses

Your agents don't care which one they're talking to.

* * *

What would the stub actually do?
--------------------------------

You could make it return realistic structured responses based on the request.

For example:

C#

    public class StubChatClient : IChatClient
    {
        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            // Inspect the request and return a predetermined response.
        }
    }

Then when your Requirements Agent asks:

> Analyze this requirement and identify ambiguities.

the stub could return:

JSON

    {
      "requirements": [
        "Create shortened URLs",
        "Redirect users",
        "Track analytics"
      ],
      "ambiguities": [
        "URL expiration behavior is unspecified",
        "Analytics retention period is unspecified"
      ],
      "assumptions": [
        "Anonymous URL creation is allowed"
      ]
    }

Your **orchestrator doesn't know or care that this didn't come from an actual LLM**.

* * *

But there's an even better approach for your assignment
=======================================================

I'd create **three LLM modes**:

    AI Mode
    │
    ├── Stub
    ├── Scripted
    └── Real

### 1\. Stub mode

Completely deterministic.

Used for:

*   unit tests
    
*   automated demonstrations
    
*   failure testing
    
*   repeatability
    

Example:

    AI_MODE=stub

* * *

### 2\. Scripted mode

This is particularly useful for demonstrating your orchestration capabilities.

You deliberately tell the fake model:

    Scenario: TestingFailure

and it responds:

    Attempt 1 → Tests fail
    Attempt 2 → Tests fail
    Attempt 3 → Tests pass

Now you can demonstrate:

    Agent
      ↓
    Failure
      ↓
    Retry
      ↓
    Failure
      ↓
    Retry
      ↓
    Success

without relying on an actual LLM to randomly fail at the right moment.

You can similarly script:

    Scenario: SecurityViolation
    Scenario: HumanApprovalRequired
    Scenario: ArchitectureChanged
    Scenario: ImplementationFailure
    Scenario: Rollback

This is actually **better for demonstrating governance** than relying on a real model.

* * *

### 3\. Real mode

Then you can optionally support:

    AI_MODE=openai

or whatever provider you eventually choose.

You don't need this to develop the entire system.

* * *

There's an important interview distinction here
===============================================

I would **not try to hide the fact that you're using a stub**.

Instead, explicitly explain it:

> "The orchestration system is model-independent. For deterministic development and testing, I implemented a stub LLM provider that produces controlled responses. This allows me to test retry, rollback, policy violations, human approval, and re-planning deterministically. A production implementation can replace the stub with an actual LLM provider without changing the orchestration layer."

That's a **good engineering decision**, not a shortcut.

It demonstrates separation of concerns.

* * *

You can even test the agents without AI
=======================================

Microsoft's .NET guidance explicitly recommends mocking external AI services in unit tests because model output is nondeterministic and external AI calls make tests slower and environmentally dependent. [![](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com&sz=128)Microsoft Learn](https://learn.microsoft.com/en-us/microsoft-365/agents-sdk/unit-testing-agents-dotnet?utm_source=chatgpt.com)

So your test suite can do:

    RequirementsAgent
           │
           ▼
       Stub LLM
           │
           ▼
    Known response
           │
           ▼
    Assert requirements

while your integration/demo environment can do:

    RequirementsAgent
           │
           ▼
       Stub LLM
           │
           ▼
    Orchestrator
           │
           ▼
    URL Shortener

No Internet required.

* * *

And there's another option
==========================

You could also run an actual **local LLM** instead of using a fake one.

The architecture would then be:

    Visual Studio
         │
         ▼
    Your .NET Application
         │
         ▼
    IChatClient
         │
         ▼
    Local LLM

That's useful if you want the agents to genuinely reason rather than use predetermined responses.

But I **wouldn't start there**.

For a 2–3 day assignment, I'd build:

**Phase 1:**

    Stub LLM → get entire orchestration system working

**Phase 2:**

    Optional local/real LLM → demonstrate genuine agent behavior

That way you're not blocked by AI infrastructure.

* * *

In fact, I'd structure your project this way
--------------------------------------------

    AgenticEngineeringSystem
    │
    ├── Agents
    │   ├── RequirementsAgent
    │   ├── ArchitectureAgent
    │   ├── PlanningAgent
    │   ├── ImplementationAgent
    │   ├── TestingAgent
    │   └── ReviewAgent
    │
    ├── Orchestration
    │   ├── Workflow
    │   ├── TaskGraph
    │   ├── WorkflowState
    │   └── WorkflowExecutor
    │
    ├── AI
    │   ├── IAgentModel
    │   ├── StubAgentModel
    │   └── RealAgentModel
    │
    ├── Governance
    │   ├── PolicyEngine
    │   ├── ApprovalService
    │   └── AuditService
    │
    └── URLShortener

The crucial abstraction is something along the lines of:

C#

    public interface IAgentModel
    {
        Task<T> GenerateAsync<T>(
            AgentRequest request,
            CancellationToken cancellationToken);
    }

Then:

C#

    public class StubAgentModel : IAgentModel
    {
        // Deterministic responses
    }

and eventually:

C#

    public class OpenAiAgentModel : IAgentModel
    {
        // Actual LLM
    }

You can even leave `OpenAiAgentModel` completely unimplemented for the interview prototype.

* * *

### One thing I'd change from my previous answer

I previously suggested that an LLM API was **required**. That's too strong.

**The assignment requires agentic behavior, but it does not explicitly require you to purchase access to a commercial LLM API.**

A deterministic stub can demonstrate the orchestration architecture, provided you clearly distinguish **simulated agent reasoning** from genuine model inference.

And given the assignment's emphasis on **workflow orchestration, governance, retries, rollback, auditability, and controlled autonomy**, a deterministic stub is actually extremely useful because it lets you demonstrate those things reliably.

You can therefore build the bulk of this entirely in Visual Studio with **zero cloud AI expenditure**, and optionally plug a real model into the same interface later. Microsoft.Extensions.AI is specifically designed around this kind of provider abstraction. [![](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com&sz=128)Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/ai/dotnet-ai-ecosystem?utm_source=chatgpt.com)

If you want, I can next show you **exactly how I'd implement the stubbed agent architecture in C#**, including a small `IChatClient` implementation, the orchestrator, task graph, and a couple of simulated failures. That would give us a concrete starting point for building this assignment.
