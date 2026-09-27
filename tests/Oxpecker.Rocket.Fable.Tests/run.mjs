// Contract tests use a stub runtime, not a simulated browser or the real Rocket bundle.
import { registerHooks } from 'node:module';

registerHooks({
    resolve(specifier, context, nextResolve) {
        if (specifier === 'datastar-rocket') {
            return { url: new URL('./rocket-stub.mjs', import.meta.url).href, shortCircuit: true };
        }
        return nextResolve(specifier, context);
    }
});

await import('./dist/Program.mjs');
