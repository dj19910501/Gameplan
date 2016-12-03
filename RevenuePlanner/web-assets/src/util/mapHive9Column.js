import roDate from 'gridCellTypes/roDate';
/**
 * These keys correspond to the values of the HeaderMappingFormat enum in ITransaction.cs
 * The values correspond to column definitions
 */
const headerMappingFormat = {
    /**
     * Label - "short" text
     */
    0: {
        width: 150,
        type: ["rotxt", "edtxt"],
        align: "left",
        sort: "str",
        hidden: false,
        noresize: false,
    },
    /**
     * Text - "long" text
     */
    1: {
        width: "*",
        type: ["rotxt", "edtxt"],
        align: "left",
        sort: "str",
        hidden: false,
        noresize: false,
    },
    /**
     * Date
     */
    2: {
        width: 90,
        type: [roDate, "dhxCalendarA"],
        align: "left",
        sort: "date",
        hidden: false,
        noresize: false,
    },
    /**
     * Currency
     */
    3: {
        width: 90,
        type: ["ron", "edn"],
        numberFormat: `${window.CurrencySybmol} 0,000.00`,
        align: "right",
        sort: "int",
        hidden: false,
        noresize: false,
    },
    /**
     * Number
     */
    4: {
        width: 90,
        type: ["ron", "edn"],
        numberFormat: "0,000.00",
        align: "right",
        sort: "int",
        hidden: false,
        noresize: false,
    },
    /** Percent
     *
     */
    5: {
        width: 50,
        type: ["ron", "edn"],
        numberFormat: `0,000 %`,
        align: "right",
        sort: "int",
        hidden: false,
        noresize: false,
    },
    /**
     * Identifier
     */
    6: {
        width: 50,
        type: ["rotxt", "edtxt"],
        align: "left",
        sort: "str",
        hidden: false,
        noresize: false,
    }
};

/**
 * Converts a standard Hive9 Column Header Mapping value into a gridDataSource Column Definition
 * @param mapping
 * @param editable false to produce a column mapping for a readonly column, or true if the cells can be edited
 */
export default function mapHive9Column(mapping, editable) {
    const entry = headerMappingFormat[mapping.HeaderFormat] || headerMappingFormat[0];

    return {
        ...entry,
        id: mapping.Hive9Header,
        value: mapping.ClientHeader,
        type: entry.type[editable ? 1 : 0],
    };
}
