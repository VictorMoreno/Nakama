import { Component, type ErrorInfo, type ReactNode } from "react";
import ErrorPage from "../../pages/error/ErrorPage";

interface ErrorBoundaryProps {
  children: ReactNode;
}

interface ErrorBoundaryState {
  error: Error | null;
}

// Class component: getDerivedStateFromError/componentDidCatch have no hook equivalent yet -
// this is the one place in the app a class component is the right tool, not a stylistic choice.
export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
  state: ErrorBoundaryState = { error: null };

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { error };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error("Unhandled render error", error, info.componentStack);
  }

  render() {
    if (this.state.error) {
      return <ErrorPage error={this.state.error} />;
    }
    return this.props.children;
  }
}
