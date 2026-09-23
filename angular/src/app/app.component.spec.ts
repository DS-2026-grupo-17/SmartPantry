import { describe, it, expect } from 'vitest';
import { AppComponent } from './app.component';

describe('AppComponent', () => {
  it('should create an instance', () => {
    const app = new AppComponent();
    expect(app).toBeTruthy();
  });
});