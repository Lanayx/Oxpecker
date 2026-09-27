import assert from 'node:assert/strict';

const registrations = new Map();
export function rocket(tag, options) {
    assert.equal(typeof tag, 'string');
    registrations.set(tag, options);
}
export function getRegistration(tag) { return registrations.get(tag); }

export function renderContext() {
    const arrays = new Set();
    return {
        props: {}, host: {},
        html(strings, ...values) {
            assert(Array.isArray(strings));
            assert(Array.isArray(strings.raw));
            assert.equal(strings.length, 1);
            assert.equal(strings[0], strings.raw[0]);
            assert.equal(values.length, 0);
            assert(Object.isFrozen(strings));
            assert(Object.isFrozen(strings.raw));
            assert.equal(Object.getOwnPropertyDescriptor(strings, 'raw').enumerable, false);
            assert(!arrays.has(strings), 'Changed markup must not reuse a template identity');
            arrays.add(strings);
            return { markup: strings[0] };
        }
    };
}

export function setupContext() {
    const context = {
        props: { start: 3 }, $$: {}, $: {}, host: {}, refs: {},
        names: [], stopped: 0, cleaned: 0,
        cleanup(callback) { callback(); context.cleaned++; },
        effect(callback) { callback(); return () => context.stopped++; },
        observeProps(callback, ...names) {
            context.names = names;
            callback();
            return () => context.stopped++;
        },
        action(name, callback) {
            context.actionName = name;
            callback({ host: context.host, props: context.props, state: context.$$, el: null, evt: undefined });
        }
    };
    return context;
}

// Record the JS calls so the F# tests can verify member names, arguments, and spreading.
export function codecRegistry() {
    const calls = [];
    function codec(kind) {
        let proxy;
        proxy = new Proxy({}, {
            get(_, name) {
                if (['trim', 'upper', 'lower', 'kebab', 'camel', 'snake', 'pascal', 'title', 'round'].includes(name)) {
                    calls.push([kind, name]); return proxy;
                }
                return (...args) => {
                    calls.push([kind, name, ...args.map(x => typeof x === 'function' ? x() : x)]);
                    return proxy;
                };
            }
        });
        return proxy;
    }
    return {
        calls,
        string: codec('string'), number: codec('number'), bool: codec('bool'),
        date: codec('date'), json: codec('json'), js: codec('js'), bin: codec('bin'),
        array(item) { calls.push(['array', item]); return codec('array'); },
        object(shape) { calls.push(['object', shape]); return codec('object'); },
        oneOf(...values) { calls.push(['oneOf', ...values]); return codec('oneOf'); }
    };
}
