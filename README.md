# XtremeIdiots Portal - Event Ingest

[![Build and Test](https://github.com/frasermolyneux/portal-event-ingest/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/frasermolyneux/portal-event-ingest/actions/workflows/build-and-test.yml)
[![Code Quality](https://github.com/frasermolyneux/portal-event-ingest/actions/workflows/codequality.yml/badge.svg)](https://github.com/frasermolyneux/portal-event-ingest/actions/workflows/codequality.yml)
[![PR Verify](https://github.com/frasermolyneux/portal-event-ingest/actions/workflows/pr-verify.yml/badge.svg)](https://github.com/frasermolyneux/portal-event-ingest/actions/workflows/pr-verify.yml)
[![Deploy Dev](https://github.com/frasermolyneux/portal-event-ingest/actions/workflows/deploy-dev.yml/badge.svg)](https://github.com/frasermolyneux/portal-event-ingest/actions/workflows/deploy-dev.yml)
[![Deploy Prd](https://github.com/frasermolyneux/portal-event-ingest/actions/workflows/deploy-prd.yml/badge.svg)](https://github.com/frasermolyneux/portal-event-ingest/actions/workflows/deploy-prd.yml)
[![Destroy Development](https://github.com/frasermolyneux/portal-event-ingest/actions/workflows/destroy-development.yml/badge.svg)](https://github.com/frasermolyneux/portal-event-ingest/actions/workflows/destroy-development.yml)
[![Destroy Environment](https://github.com/frasermolyneux/portal-event-ingest/actions/workflows/destroy-environment.yml/badge.svg)](https://github.com/frasermolyneux/portal-event-ingest/actions/workflows/destroy-environment.yml)
[![Update Dashboard from Staging](https://github.com/frasermolyneux/portal-event-ingest/actions/workflows/update-dashboard-from-staging.yml/badge.svg)](https://github.com/frasermolyneux/portal-event-ingest/actions/workflows/update-dashboard-from-staging.yml)
[![Copilot Setup Steps](https://github.com/frasermolyneux/portal-event-ingest/actions/workflows/copilot-setup-steps.yml/badge.svg)](https://github.com/frasermolyneux/portal-event-ingest/actions/workflows/copilot-setup-steps.yml)
[![Dependabot Automerge](https://github.com/frasermolyneux/portal-event-ingest/actions/workflows/dependabot-automerge.yml/badge.svg)](https://github.com/frasermolyneux/portal-event-ingest/actions/workflows/dependabot-automerge.yml)

## Documentation

* [Development Workflows](/docs/development-workflows.md) - Branch strategy, CI/CD triggers, and development flows

## Overview

.NET 9 isolated Azure Functions app that ingests Portal events. HTTP triggers accept player, server, chat, map-change, and map-vote events then enqueue them to Service Bus queues; processors hydrate data through the Portal Repository API, cache player lookups, and emit Application Insights telemetry. Terraform and GitHub Actions cover infrastructure and deploys (Dev/Prd) plus lifecycle workflows for destroy and dashboard refresh.

## Contributing

Please read the [contributing](CONTRIBUTING.md) guidance; this is a learning and development project.

## Security

Please read the [security](SECURITY.md) guidance; I am always open to security feedback through email or opening an issue.
