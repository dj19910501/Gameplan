/**
 * Takes the supplied values and joins them together with " > " to make a breadcrumb
 * path (e.g. "Plan > Campaign > Program > Tactic")
 * Automatically removes values that are empty (just spaces or null)
 * @param args
 */
export default function breadcrumb(...args) {
    return args.filter(value => value && /\S/.test(value)).join(" > ");
}
